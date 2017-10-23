using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using PanelTrackerPlugin;

namespace PanelTrackerPlugin
{
    public class Station
    {

        public bool Outfitting {get; private set;}

        private int[] positions={0,0,0};//Left, Middle, Right

        private StarportPanels currentColumn = StarportPanels.Left;

        public List<string>[] board;

        public string name;

        public List<string> services;

        private dynamic vaProxy;

        public Station(string name, ref dynamic vaProxy, object[] services){
            string[] strings=new string[services.Length];
            int ctr=0;
            foreach (object s in services)
            {
                strings[ctr++] =(string) s;
            }
            this.vaProxy  = vaProxy;
            this.name     = name;
            this.services = processServices(strings);
            this.board    = generateBoard();
        }

        public Station(string name, ref dynamic vaProxy, string[] services)
        {
            // vaProxy.WriteToLog(services.ToString(),"orange");
            
            this.vaProxy  = vaProxy;
            this.name     = name;
            this.services = processServices(services);
            this.board    = generateBoard();
            string left="",mid="",right="";
            foreach(string s in board[0]){
                left+=s+" ";
            }
            foreach(string s in board[1]){
                mid+=s+" ";
            }
            foreach(string s in board[2]){
                right+=s+" ";
            }
        }

        public string generateAction(string target,dynamic vaProxy){
            string output="";
            vaProxy.WriteToLog(target,"orange");
            if(board[0].Contains(target)){
                vaProxy.WriteToLog("Left Panel","orange");
                if(currentColumn!=StarportPanels.Left){
                    output+=(currentColumn-StarportPanels.Left).ToString()+(char)Action.left+";";
                }
                int cpos = positions[0],              //current position
                    tpos = board[0].IndexOf(target);  //target position
                switch(cpos.CompareTo(tpos)){
                case 1:
                    output+=(cpos-tpos).ToString()+(char)Action.up+";";
                    break;
                case -1:
                    output+=(tpos-cpos).ToString()+(char)Action.down+";";
                    break;
                } 
                currentColumn=StarportPanels.Left;
                positions[0]=tpos;
            }
            else if(board[1].Contains(target)){
                switch(currentColumn){
                case StarportPanels.Left:
                    output+="1"+(char)Action.right+";";
                    break;
                case StarportPanels.Right:
                    output+="1"+(char)Action.left+";";
                    break;
                }
                int cpos = positions[1],              //current position
                    tpos = board[1].IndexOf(target);  //target position
                switch(cpos.CompareTo(tpos)){
                case 1:
                    output+=(cpos-tpos).ToString()+(char)Action.up;
                    break;
                case -1:
                    output+=(tpos-cpos).ToString()+(char)Action.down;
                    break;
                }
                currentColumn=StarportPanels.Middle;
                positions[1]=tpos;
            }
            else if(board[2].Contains(target)){
                if(currentColumn!=StarportPanels.Right){
                    output+=(StarportPanels.Right-currentColumn).ToString()+(char)Action.right+";";
                }
                int cpos = positions[2],              //current position
                    tpos = board[2].IndexOf(target);  //target position
                switch(cpos.CompareTo(tpos)){
                case 1:
                    output+=(cpos-tpos).ToString()+(char)Action.up+";";
                    break;
                case -1:
                    output+=(tpos-cpos).ToString()+(char)Action.down+";";
                    break;
                }
                currentColumn=StarportPanels.Right;
                positions[2]=tpos;
            } else {
                vaProxy.WriteToLog(target+" was not found at this starport","red");
                return "";
            }
            vaProxy.WriteToLog((output??""),"orange");
            return output+"1a";
        }

        private List<string> processServices(string[] services)
        {
            List<string> output = new List<string>();
            foreach (string s in services)
            {
                switch (s)
                {
                    case "Exploration"      : 
                    case "Missions"         : 
                    case "Refuel"           : 
                    case "MissionsGenerated": 
                    case "Shipyard"         : 
                    case "Contacts"         : 
                    case "Commodities"      : 
                        output.Add(s);
                        break;
                }
            }
            return output;
        }
        public List<string>[] generateBoard()
        {
            List<string>[] output = new List<string>[] { new List<string>(), new List<string>(new string[] { "Galnet" }), new List<string>() };
            output[0]=sortListLeft(services.ToArray());
            output[2]=sortListRight(services.ToArray());
            return output;
        }
        private List<string> sortListLeft(string[] Commodities)
        {
            List<string> output=new List<string>();
            output.Add(leftPanelOrder[6]);
            foreach(string commodity in Commodities){
                switch(commodity){
                case "Commodities":
                    output.Add(leftPanelOrder[0]);
                    break;
                case "Missions":
                    output.Add(leftPanelOrder[1]);
                    output.Add(leftPanelOrder[2]);
                    break;
                case "Contacts":
                    output.Add(leftPanelOrder[3]); 
                    break;
                case "Exploration":
                    output.Add(leftPanelOrder[4]);
                    break;
                case "CrewLounge":
                    output.Add(leftPanelOrder[5]);
                    break;
                }
            }
            output.Sort((string first,string second)=>{
                return leftPanelOrder.IndexOf(first).CompareTo(leftPanelOrder.IndexOf(second));
            });
            return output;
        }

        private List<string> sortListRight(string[] Commodities)
        {
            List<string> output=new List<string>();
            output.Add(rightPanelOrder[0]);
            output.Add(rightPanelOrder[1]);
            foreach(string commodity in Commodities){
                switch(commodity){
                case "Rearm":
                    output.Add(rightPanelOrder[4]);
                    break;
                case "Refuel":
                    output.Add(rightPanelOrder[2]);
                    break;
                case "Repair":
                    output.Add(rightPanelOrder[3]);
                    output.Add(rightPanelOrder[7]);
                    break;
                case "Outfitting":
                    output.Add(rightPanelOrder[5]);
                    Outfitting=true;
                    break;
                case "Shipyard":
                    output.Add(rightPanelOrder[6]);
                    break;
                }
            }
            output.Sort((string first,string second)=>{
                return rightPanelOrder.IndexOf(first).CompareTo(rightPanelOrder.IndexOf(second));
            });
            return output;
        }

        private ReadOnlyCollection<string> leftPanelOrder {get;} = new ReadOnlyCollection<string>(new string[]{
            "Commodities",
            "Missions",
            "PassengerLounge",
            "Contacts",
            "Cartographics",
            "CrewLounge",
            "Exit"
        });

        private ReadOnlyCollection<string> rightPanelOrder {get;} =new ReadOnlyCollection<string>(new string[]{
             "HoloMe",
             "Livery",
             "Refuel",
             "BasicRepair",
             "Restock",
             "Outfitting",
             "Shipyard",
             "AdvancedRepair"
        });

        public override string ToString(){
            string contains="";
            foreach(string s in services){
                contains+=s+", ";
            }
            contains=contains.Length>0?contains.Substring(0,contains.Length-2):"nothing";
            return this.name??"default"+" has "+contains+".";
        }
    }
}