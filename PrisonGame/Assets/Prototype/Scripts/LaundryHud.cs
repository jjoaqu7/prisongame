using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
namespace PrisonGame.Prototype
{
    // Local presentation only. Existing inventory/assignment owners remain authoritative.
    public sealed class LaundryHud : MonoBehaviour
    {
        LaundryDuty duty; FirstPersonController movement; PlayerInteraction actor; SnackSupplies supplies;
        SnackLoopHud snack; GuardSuspicionHud guard; PlayerInventory inventory; int selected;
        GUIStyle body, title, small, prompt, panelStyle; Texture2D rounded;
        readonly Queue<Notice> notices=new Queue<Notice>(); Notice current;
        Notice dialogue;
        sealed class Notice { public string text; public float left,total; }
        bool lastWork; LaundryDuty.Phase lastPhase; string lastFavor; float reveal=5;
        public bool Focused=>duty.InLaundry || duty.Data.phase==LaundryDuty.Phase.Active;
        public bool InventoryVisible=>movement.ControlsActive && Keyboard.current!=null && Keyboard.current.tabKey.isPressed;
        public bool WarningVisible=>duty.Data.phase==LaundryDuty.Phase.Active && duty.Data.grace>0;
        public string NoticeText=>current?.text;
        public string DialogueText=>dialogue?.text;
        public string Favor=>duty.Data.officerKeyGiven?"Harris's key - cell next to yours":duty.Data.pokerSecret?"Blackmail Harris in the common room":duty.Data.contactMet?"Ask Dex about Harris":duty.Data.favorDone?"Meet Dex":duty.Data.hasTool?"Return the valve key to Rue":duty.Data.favorAsked?"Find Rue's valve key":"Talk to Rue";
        public string[] PocketRows {
            get {var rows=new List<string>();
                foreach(var entry in inventory.Entries)
                    if(entry.item==null || actor.HeldItem!=entry.item)rows.Add(entry.name+"|"+entry.count);
                return rows.ToArray();}
        }
        void Awake(){inventory=GetComponent<PlayerInventory>();duty=GetComponent<LaundryDuty>();movement=GetComponent<FirstPersonController>();actor=GetComponent<PlayerInteraction>();supplies=GetComponent<SnackSupplies>();snack=GetComponent<SnackLoopHud>();guard=GetComponent<GuardSuspicionHud>();}
        public void Notify(string text,float duration=3)
        {
            if(string.IsNullOrEmpty(text))return;
            if(WarningVisible && text.StartsWith("Vale: Back inside"))return;
            // Repeated identical messages refresh rather than stacking duplicates.
            float seconds=Mathf.Clamp(duration,3,12);
            if(current!=null && current.text==text){current.left=current.total=seconds;return;}
            var n=new Notice{text=text,left=seconds,total=seconds};
            if(current==null)current=n;else{if(notices.Count>=3)notices.Dequeue();notices.Enqueue(n);}
        }
        public void Speak(string text,float duration=8)
        {
            if(string.IsNullOrEmpty(text))return;
            // Direct conversations replace the previous line immediately, never enter the toast queue.
            float seconds=Mathf.Clamp(duration,4,12);
            dialogue=new Notice{text=text,left=seconds,total=seconds};
        }
        public void ClearNotices(){current=null;dialogue=null;notices.Clear();reveal=5;}
        void Update()
        {
            snack.enabled=!Focused&&!InventoryVisible;guard.enabled=!Focused&&!InventoryVisible;
            bool working=duty.room.AtWork(actor);string favor=Favor;
            if(working!=lastWork||duty.Data.phase!=lastPhase||favor!=lastFavor)reveal=5;
            lastWork=working;lastPhase=duty.Data.phase;lastFavor=favor;
            if(!movement.ControlsActive)return;
            if(InventoryVisible)
            {
                var keyboard=Keyboard.current;var entries=inventory.Entries;
                Key[] keys={Key.Digit1,Key.Digit2,Key.Digit3,Key.Digit4,Key.Digit5,Key.Digit6};
                for(int i=0;i<keys.Length;i++)if(keyboard[keys[i]].wasPressedThisFrame)selected=i;
                selected=Mathf.Clamp(selected,0,Mathf.Max(0,entries.Count-1));
                var entry=entries.Count>0?entries[selected]:null;
                if(keyboard.rKey.wasPressedThisFrame)inventory.PocketHeld(actor);
                if(entry!=null && keyboard.fKey.wasPressedThisFrame && entry.item!=null)inventory.Hold(actor,entry.item);
                if(entry!=null && keyboard.qKey.wasPressedThisFrame && entry.item!=null)inventory.Drop(actor,entry.item);
            }
            reveal=Mathf.Max(0,reveal-Time.unscaledDeltaTime);
            if(dialogue!=null && !InventoryVisible){dialogue.left-=Time.unscaledDeltaTime;if(dialogue.left<=0)dialogue=null;}
            if(current!=null && dialogue==null && !WarningVisible && !InventoryVisible){current.left-=Time.unscaledDeltaTime;if(current.left<=0)current=notices.Count>0?notices.Dequeue():null;}
        }
        void OnDestroy(){if(rounded!=null)Destroy(rounded);}
        void Styles()
        {
            if(body!=null)return;
            body=new GUIStyle(GUI.skin.label){fontSize=18,wordWrap=true};body.normal.textColor=new Color(.94f,.93f,.87f);
            title=new GUIStyle(body){fontSize=21,fontStyle=FontStyle.Bold};small=new GUIStyle(body){fontSize=15};small.normal.textColor=new Color(.77f,.79f,.74f);
            rounded=new Texture2D(32,32,TextureFormat.RGBA32,false);rounded.filterMode=FilterMode.Bilinear;rounded.wrapMode=TextureWrapMode.Clamp;
            for(int y=0;y<32;y++)for(int x=0;x<32;x++){float dx=Mathf.Max(7-x,0,x-24),dy=Mathf.Max(7-y,0,y-24);rounded.SetPixel(x,y,new Color(1,1,1,Mathf.Clamp01(8-Mathf.Sqrt(dx*dx+dy*dy))));}rounded.Apply();
            panelStyle=new GUIStyle{border=new RectOffset(8,8,8,8)};panelStyle.normal.background=rounded;
            prompt=new GUIStyle(body){alignment=TextAnchor.MiddleCenter};
        }
        void Text(Rect r,string text,GUIStyle style,Color? color=null)
        {
            var original=style.normal.textColor;style.normal.textColor=new Color(0,0,0,.8f);GUI.Label(new Rect(r.x+1,r.y+1,r.width,r.height),text,style);
            style.normal.textColor=color??original;GUI.Label(r,text,style);style.normal.textColor=original;
        }
        void Fill(Rect r,Color color){var old=GUI.color;GUI.color=color;GUI.DrawTexture(r,Texture2D.whiteTexture);GUI.color=old;}
        void Panel(Rect r){var old=GUI.color;GUI.color=new Color(.10f,.115f,.105f,.94f*old.a);GUI.Box(r,GUIContent.none,panelStyle);GUI.color=old;}
        public static string Timer(float seconds){int n=Mathf.CeilToInt(seconds);return (n/60)+":"+(n%60).ToString("00");}
        void OnGUI()
        {
            if(!movement.ControlsActive)return;Styles();
            var matrix=GUI.matrix;var color=GUI.color;float scale=Mathf.Clamp(Screen.height/900f,.75f,2.5f);GUI.matrix=Matrix4x4.Scale(Vector3.one*scale);
            float width=Screen.width/scale,height=Screen.height/scale;var d=duty.Data;
            if(Focused)
            {
                bool active=d.phase==LaundryDuty.Phase.Active;
                if(active){Text(new Rect(width-175,28,150,28),"Shift "+Timer(d.remaining),body,d.remaining<=20?new Color(1,.73f,.35f):(Color?)null);
                    if(d.warned&&!WarningVisible)Text(new Rect(width-210,60,185,27),"Warning used",small,new Color(1,.73f,.35f));}
                if(WarningVisible){Fill(new Rect(24,30,3,66),new Color(1,.7f,.28f));Text(new Rect(38,28,330,32),"Return to work - "+Mathf.CeilToInt(d.grace)+"s",title,new Color(1,.73f,.35f));Text(new Rect(38,64,330,28),"Another breach fails this shift",small);}
                else if(InventoryVisible || (active&&duty.room.AtWork(actor)) || reveal>0 || d.phase==LaundryDuty.Phase.Unreported)
                {
                    Text(new Rect(28,28,330,32),"Laundry",title);
                    if(active){Text(new Rect(28,64,320,28),"Work "+Mathf.FloorToInt(d.worked)+" / 60s"+(duty.room.AtWork(actor)?"":" - paused"),body);
                        Fill(new Rect(28,100,220,5),new Color(.12f,.15f,.13f,.7f));Fill(new Rect(28,100,220*Mathf.Clamp01(d.worked/LaundryDuty.WorkSeconds),5),new Color(.61f,.77f,.61f));
                        if(reveal>0||InventoryVisible)Text(new Rect(28,117,340,50),Favor,body);}
                    else Text(new Rect(28,66,340,58),d.phase==LaundryDuty.Phase.Unreported?"Report at the entrance board":d.phase==LaundryDuty.Phase.Completed?"Shift complete - relieved":"Shift failed - report to retry",body);
                }
                if(actor.Target!=null&&!InventoryVisible)Text(new Rect(width/2-300,height/2+38,600,64),actor.Target.Prompt(actor),prompt);
                if(actor.HeldItem!=null&&!InventoryVisible)Text(new Rect(width-320,height-80,292,44),"Holding "+actor.HeldItem.ItemName+" - R pocket / Q put down",small);
            }
            if(InventoryVisible)DrawInventory(width,height);
            else if(dialogue!=null)
            {
                float w=Mathf.Min(650,width-56),h=body.CalcHeight(new GUIContent(dialogue.text),w-44)+34;
                float x=(width-w)/2,y=Mathf.Min(height*.68f,height-76-h);
                Panel(new Rect(x,y,w,h));
                Fill(new Rect(x+14,y+17,3,h-34),new Color(.61f,.77f,.61f));
                Text(new Rect(x+28,y+17,w-48,h-30),dialogue.text,body);
            }
            else if(current!=null&&!WarningVisible)
            {
                float w=Mathf.Min(510,width-56),h=body.CalcHeight(new GUIContent(current.text),w-44)+30;
                GUI.color=new Color(1,1,1,Mathf.Clamp01(current.left/.4f));Panel(new Rect(28,height-90-h,w,h));Fill(new Rect(42,height-75-h,3,h-30),new Color(.61f,.77f,.61f));
                Text(new Rect(54,height-76-h,w-42,h-26),current.text,body);GUI.color=color;
            }
            if(Focused&&!InventoryVisible)Text(new Rect(28,height-43,310,26),"Hold Tab - inventory & task",small);
            GUI.matrix=matrix;GUI.color=color;
        }
        void DrawInventory(float width,float height)
        {
            float w=Mathf.Min(360,width*.40f),x=width-w-26,y=105,h=Mathf.Min(680,height-145);Panel(new Rect(x,y,w,h));
            Text(new Rect(x+22,y+18,w-44,35),"Inventory   "+inventory.Used+" / 6",title);
            Text(new Rect(x+22,y+60,w-44,30),"1-6 select  /  stacks share a slot",small);
            var entries=inventory.Entries;float rowY=y+103;
            for(int i=0;i<PlayerInventory.Capacity;i++)
            {
                var entry=i<entries.Count?entries[i]:null;
                if(entry!=null && i==selected)Fill(new Rect(x+12,rowY-2,w-24,40),new Color(.53f,.64f,.5f,.14f));
                Text(new Rect(x+22,rowY,25,34),(i+1).ToString(),small);
                Text(new Rect(x+50,rowY,w-115,34),entry!=null?entry.name:"-",body);
                if(entry!=null)Text(new Rect(x+w-62,rowY,45,34),"x"+entry.count,small);
                rowY+=44;
            }
            selected=Mathf.Clamp(selected,0,Mathf.Max(0,entries.Count-1));
            Fill(new Rect(x+22,rowY+5,w-44,1),new Color(.5f,.52f,.47f,.3f));
            Text(new Rect(x+22,rowY+18,w-44,52),entries.Count>0?entries[selected].detail:"Pick up a parcel or snack pack with E.",small);
            Text(new Rect(x+22,rowY+79,w-44,50),"In hand: "+(actor.HeldItem!=null?actor.HeldItem.ItemName:"nothing")+"\nR - pocket held item",small);
            Text(new Rect(x+22,rowY+143,w-44,55),Favor,body);
            Text(new Rect(x+22,y+h-55,w-44,50),"Release Tab to close\nWorld time continues",small);
        }
    }
}
