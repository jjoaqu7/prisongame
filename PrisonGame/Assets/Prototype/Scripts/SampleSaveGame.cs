using System;
using System.IO;
using System.Linq;
using UnityEngine;

namespace PrisonGame.Prototype
{
    [Serializable]
    public sealed class SampleSaveData
    {
        public int version;
        public string scene;
        public Personal player;
        public World world;
        [Serializable] public sealed class Personal
        {
            public Vector3 position;
            public float yaw, pitch, suspicion;
            public int money, sales, crackers, fruit, wrappers, delivered, paid;
            public bool starter, accepted, completedLate;
            public double dueAt;
            public LaundryDuty.State laundry;
        }
        [Serializable] public sealed class World
        {
            public double minutes, reopensAt;
            public bool inspectionStarted, shelfInstalled;
            public int trayStage;
            public Item[] parcels, packs;
            public Door[] doors;
            public Door neighboringCell;
            public LaundryRoom.State laundryRoom;
        }
        [Serializable] public sealed class Item
        {
            public Vector3 position, velocity, angularVelocity;
            public Quaternion rotation;
            public bool held, pocketed;
        }
        [Serializable] public sealed class Door { public Vector3 position; public bool opening, unlocked; }
    }

    // One sample's persistence boundary. Personal and shared state are distinct;
    // this is not a multiplayer save format. Local menu/input remains outside rules.
    [RequireComponent(typeof(PlayerInteraction), typeof(SnackSupplies))]
    public sealed class SampleSaveGame : MonoBehaviour
    {
        [SerializeField] private SnackAssembly assembly;
        [SerializeField] private SnackShelf shelf;
        [SerializeField] private SupplyInspection inspection;
        [SerializeField] private PrototypePickup[] parcels;
        [SerializeField] private PrototypeDoor[] doors;
        [SerializeField] private PrototypeDoor neighboringCell;
        private PlayerInteraction actor;
        private FirstPersonController movement;
        private SnackSupplies stock;
        private SnackRequest request;
        private GuardSuspicion guard;
        public string Status { get; private set; } = "Manual save only. Load replaces current progress.";
        public string SavePath => Path.Combine(Application.persistentDataPath, GetComponent<LaundryDuty>() != null ? "laundry-save-v1.json" : "sample-save-v1.json");

        private void Awake()
        {
            actor = GetComponent<PlayerInteraction>(); movement = GetComponent<FirstPersonController>();
            stock = GetComponent<SnackSupplies>(); request = GetComponent<SnackRequest>();
            guard = GetComponent<GuardSuspicion>();
        }

        public void DrawMenu(GUIStyle label, GUIStyle button)
        {
            GUILayout.Space(8);
            GUILayout.Label(Status, label, GUILayout.Height(54));
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Save progress", button, GUILayout.Height(36))) Save(actor);
            bool previous = GUI.enabled;
            GUI.enabled = previous && File.Exists(SavePath);
            if (GUILayout.Button("Load save", button, GUILayout.Height(36))) Load(actor);
            GUI.enabled = previous;
            GUILayout.EndHorizontal();
            GUILayout.Label("One slot. Saving replaces the previous save.", label);
        }

        private void RequirePaused(PlayerInteraction actingPlayer)
        {
            if (actingPlayer != actor || movement.ControlsActive || !request.Clock.Paused)
                throw new InvalidOperationException("Open settings before saving or loading.");
        }

        private static SampleSaveData.Item CaptureItem(PrototypePickup item, PlayerInteraction actor)
        {
            var body = item.GetComponent<Rigidbody>();
            return new SampleSaveData.Item { position = item.transform.position, rotation = item.transform.rotation,
                held = actor.HeldItem == item, pocketed = item.PocketOwner == actor, velocity = body.isKinematic ? Vector3.zero : body.linearVelocity,
                angularVelocity = body.isKinematic ? Vector3.zero : body.angularVelocity };
        }

        public SampleSaveData Capture(PlayerInteraction actingPlayer)
        {
            RequirePaused(actingPlayer);
            var packs = FindObjectsByType<SnackPackItem>(FindObjectsSortMode.None).Where(p => !p.Sold).ToArray();
            if (packs.Any(p => p.Source != stock) || (assembly.WorkingPlayer != null && assembly.WorkingPlayer != actor))
                throw new InvalidOperationException("This sample supports one player's progress only.");
            return new SampleSaveData {
                version = 1, scene = gameObject.scene.name,
                player = new SampleSaveData.Personal { position = transform.position, yaw = transform.eulerAngles.y,
                    pitch = movement.SavedPitch, suspicion = guard.Level, money = stock.Money, sales = stock.Sales,
                    crackers = stock.Crackers, fruit = stock.Fruit, wrappers = stock.Wrappers, starter = stock.StarterCollected,
                    accepted = request.Accepted, delivered = request.Delivered, paid = request.Paid,
                    dueAt = request.DueAt, completedLate = request.CompletedLate, laundry = GetComponent<LaundryDuty>()?.Capture() },
                world = new SampleSaveData.World { minutes = request.Clock.TotalMinutes,
                    inspectionStarted = inspection.Started, reopensAt = inspection.ReopensAt,
                    shelfInstalled = shelf.Installed, trayStage = assembly.Stage,
                    parcels = parcels.Select(p => CaptureItem(p, actor)).ToArray(),
                    packs = packs.Select(p => CaptureItem(p.GetComponent<PrototypePickup>(), actor)).ToArray(),
                    doors = doors.Select(d => d.Capture()).ToArray(), neighboringCell = neighboringCell!=null?neighboringCell.Capture():null, laundryRoom = GetComponent<LaundryDuty>()?.room.Capture() }
            };
        }

        private static bool Finite(double n) => !double.IsNaN(n) && !double.IsInfinity(n);
        private static bool VectorValid(Vector3 v) => Finite(v.x) && Finite(v.y) && Finite(v.z) && v.sqrMagnitude < 1000000;
        private static void Need(bool valid) { if (!valid) throw new InvalidDataException("Save is damaged or incompatible. Current progress was kept."); }
        private void Validate(SampleSaveData s)
        {
            Need(s != null && s.version == 1 && s.scene == gameObject.scene.name && s.player != null && s.world != null);
            var p = s.player; var w = s.world;
            var laundry = GetComponent<LaundryDuty>();
            if (laundry != null) Need(LaundryDuty.Valid(p.laundry) && laundry.room.Valid(w.laundryRoom));
            Need(VectorValid(p.position) && Finite(p.yaw) && Finite(p.pitch) && Math.Abs(p.pitch) <= 85);
            Need(Finite(p.suspicion) && p.suspicion >= 0 && p.suspicion <= 100);
            Need(p.money >= 0 && p.sales >= 0 && p.crackers >= 0 && p.fruit >= 0 && p.wrappers >= 0);
            Need(p.delivered >= 0 && p.delivered <= 3 && p.sales >= p.delivered && p.paid >= p.delivered * 2 && p.paid <= p.delivered * 3);
            Need(Finite(w.minutes) && w.minutes >= 480 && w.minutes <= 100000000);
            Need(Finite(p.dueAt) && (p.accepted ? p.dueAt >= 540 && p.dueAt <= w.minutes + 60.001 : p.dueAt == 0 && p.delivered == 0));
            Need(!p.completedLate || (p.delivered == 3 && w.minutes >= p.dueAt));
            Need(w.trayStage >= 0 && w.trayStage <= 2 && Finite(w.reopensAt));
            Need(w.inspectionStarted ? w.reopensAt >= 490 && w.reopensAt <= w.minutes + 10.001 : w.reopensAt == 0);
            Need(w.parcels != null && w.parcels.Length == parcels.Length && w.doors != null && w.doors.Length == doors.Length && w.packs != null && w.packs.Length <= 10000);
            int held = 0, carried = (p.crackers>0?1:0)+(p.fruit>0?1:0)+(p.wrappers>0?1:0)+
                (p.laundry!=null && p.laundry.hasTool?1:0)+(p.laundry!=null && p.laundry.hasOfficerKey?1:0);
            foreach (var item in w.parcels.Concat(w.packs))
            {
                Need(item != null && VectorValid(item.position) && VectorValid(item.velocity) && VectorValid(item.angularVelocity));
                var q = item.rotation;
                double norm = (double)q.x*q.x + (double)q.y*q.y + (double)q.z*q.z + (double)q.w*q.w;
                Need(Finite(norm) && Math.Abs(norm - 1) < .01);
                if (item.held) held++;
                if (item.held || item.pocketed) carried++;
                Need(!(item.held && item.pocketed) && (!item.pocketed || GetComponent<PlayerInventory>() != null));
            }
            Need(held <= 1);
            if(GetComponent<PlayerInventory>()!=null)Need(carried<=PlayerInventory.Capacity);
            foreach (var d in w.doors) Need(d != null && VectorValid(d.position));
            if(neighboringCell!=null)Need(neighboringCell.ValidState(w.neighboringCell));
            Need(assembly.PackPrefab != null && assembly.PackPrefab.GetComponent<SnackPackItem>() != null);
        }

        private void RestoreItem(PrototypePickup item, SampleSaveData.Item s)
        {
            item.Place(s.position);
            item.transform.rotation = s.rotation;
            if (s.held) actor.PickUp(item, false, true);
            else if (s.pocketed) item.Pocket(actor);
            else { var body = item.GetComponent<Rigidbody>(); body.linearVelocity = s.velocity; body.angularVelocity = s.angularVelocity; }
        }

        private void Apply(SampleSaveData s)
        {
            actor.ClearForRestore();
            foreach (var old in FindObjectsByType<SnackPackItem>(FindObjectsSortMode.None))
            { old.gameObject.SetActive(false); Destroy(old.gameObject); }
            movement.RestorePose(s.player.position, s.player.yaw, s.player.pitch);
            stock.Restore(s.player); request.Clock.Restore(s.world.minutes); request.Restore(s.player);
            assembly.Restore(s.world.trayStage, actor); shelf.Restore(s.world.shelfInstalled);
            inspection.Restore(s.world.inspectionStarted, s.world.reopensAt);
            for (int i = 0; i < doors.Length; i++) doors[i].Restore(s.world.doors[i]);
            if(neighboringCell!=null)neighboringCell.Restore(s.world.neighboringCell);
            for (int i = 0; i < parcels.Length; i++) RestoreItem(parcels[i], s.world.parcels[i]);
            foreach (var saved in s.world.packs)
            {
                var obj = Instantiate(assembly.PackPrefab, saved.position, saved.rotation);
                obj.GetComponent<SnackPackItem>().Initialize(stock);
                RestoreItem(obj.GetComponent<PrototypePickup>(), saved);
            }
            Physics.SyncTransforms(); guard.Restore(s.player.suspicion);
            GetComponent<SnackLoopHud>().ResetAfterLoad();
            GetComponent<GuardSuspicionHud>().ResetAfterLoad();
            GetComponent<SampleAudio>()?.ResetAfterLoad();
            var laundry = GetComponent<LaundryDuty>();
            if (laundry != null) { laundry.Restore(s.player.laundry); laundry.room.Restore(s.world.laundryRoom); }
            Time.timeScale = 0;
        }

        public void Restore(PlayerInteraction actingPlayer, SampleSaveData data)
        {
            RequirePaused(actingPlayer); Validate(data);
            var previous = Capture(actingPlayer);
            try { Apply(data); }
            catch { Apply(previous); throw; }
        }

        public bool Save(PlayerInteraction actingPlayer) => SaveTo(actingPlayer, SavePath);
        public bool Load(PlayerInteraction actingPlayer) => LoadFrom(actingPlayer, SavePath);

        // Explicit paths let automated checks use isolated fixtures, never the user's slot.
        public bool SaveTo(PlayerInteraction actingPlayer, string path)
        {
            try
            {
                var data = Capture(actingPlayer); Validate(data);
                string json = JsonUtility.ToJson(data, true);
                Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path)));
                string temp = path + ".tmp";
                using (var stream = new FileStream(temp, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    byte[] bytes = System.Text.Encoding.UTF8.GetBytes(json);
                    stream.Write(bytes, 0, bytes.Length); stream.Flush(true);
                }
                if (File.Exists(path)) File.Replace(temp, path, path + ".bak");
                else File.Move(temp, path);
                Status = "Progress saved. Resume when ready."; return true;
            }
            catch (Exception e) when (e is IOException || e is InvalidDataException || e is UnauthorizedAccessException || e is InvalidOperationException || e is ArgumentException)
            { Status = "Could not save: " + e.Message; return false; }
        }

        public bool LoadFrom(PlayerInteraction actingPlayer, string path)
        {
            try
            {
                RequirePaused(actingPlayer);
                if (new FileInfo(path).Length > 8000000) throw new InvalidDataException("Save file is too large.");
                var data = JsonUtility.FromJson<SampleSaveData>(File.ReadAllText(path));
                Restore(actingPlayer, data);
                Status = "Progress loaded. Paused until Resume walking."; return true;
            }
            catch (Exception e) when (e is IOException || e is InvalidDataException || e is UnauthorizedAccessException || e is InvalidOperationException || e is ArgumentException)
            { Status = "Could not load: " + e.Message; return false; }
        }
    }
}
