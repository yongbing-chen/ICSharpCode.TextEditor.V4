﻿using ICSharpCode.TextEditor.Actions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ICSharpCode.TextEditor.Common
{
    /// <summary>
    /// Dynamic aspect.
    /// </summary>
    public class Action
    {
        public IEditAction EditAction { get; set; } = null;

        public string Name { get; set; } = "???";

        // otherwise builtin.
        public bool UserAction { get; set; } = false;

        public List<(string, object)> Arguments { get; set; } = new List<(string, object)>();
    }

    [Serializable]
    public class ControlSpec
    {

        // { "keys": ["shift+left"], "command": "move", "args": {"by": "characters", "forward": false, "extend": true} },
        // { "keys": ["ctrl+k", "ctrl+v"], "command": "paste_from_history" },

        // {
        //     "keys": ["alt+shift+5"],
        //     "command": "set_layout",
        //     "args":
        //     {
        //         "cols": [0.0, 0.5, 1.0],
        //         "rows": [0.0, 0.5, 1.0],
        //         "cells":
        //         [
        //             [0, 0, 1, 1], [1, 0, 2, 1],
        //             [0, 1, 1, 2], [1, 1, 2, 2]
        //         ]
        //     }
        // },


        public Keys Chord1 { get; set; } = Keys.None;

        public Keys Chord2 { get; set; } = Keys.None;


        public string Menu { get; set; } = "";
        public string SubMenu { get; set; } = "";
        public bool ContextMenu { get; set; } = false;

        // TODO2 Toolbar icons.


        // any derived from IEditAction incl scripts.
        public string ActionName { get; set; }

        public ControlSpec()
        {
            Chord1 = Keys.A & Keys.Control;
            Chord2 = Keys.B;

            Menu = "file";// ToolStripMenuItem";
            SubMenu = "justATest";// ToolStripMenuItem";
            ContextMenu = false;

            ActionName = "ShiftTab";
        }
    }


    [Serializable]
    public class ControlMap
    {
        #region Properties
        /// <summary>Contents of the file.</summary>
        public List<ControlSpec> ControlSpecs { get; set; } = new List<ControlSpec>();
        #endregion

        #region Fields
        /// <summary>The file name.</summary>
        string _fn = "";
        #endregion

        #region Persistence
        /// <summary>Save object to file.</summary>
        public void Save()
        {
            string json = JsonConvert.SerializeObject(this, Formatting.Indented);
            File.WriteAllText(_fn, json);
        }

        /// <summary>Create object from file.</summary>
        public static ControlMap Load(string fname)
        {
            ControlMap ctlmap;

            //// Create dummy.
            //ctlmap = new ControlMap();
            //for (int i = 0; i < 3; i++)
            //{
            //    ctlmap.ControlSpecs.Add(new ControlSpec());
            //}
            //ctlmap._fn = fname;
            //ctlmap.Save();

            if (File.Exists(fname))
            {
                string json = File.ReadAllText(fname);
                ctlmap = JsonConvert.DeserializeObject<ControlMap>(json);
            }
            else
            {
                ctlmap = new ControlMap();
            }

            if (ctlmap != null)
            {
                ctlmap._fn = fname;
            }

            return ctlmap;
        }
        #endregion
    }

    public class ControlMapManager
    {
        #region Properties
        /// <summary>Mapping between keystrokes and actions.</summary>
        Dictionary<(Keys, Keys), Action> _keyActions = new Dictionary<(Keys, Keys), Action>();

        /// <summary>All the loaded actions, with key = name.</summary>
        Dictionary<string, IEditAction> _actions = new Dictionary<string, IEditAction>();
        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userMapFile"></param>
        /// <param name="userActionFiles">aka scripts/plugins/etc</param>
        public void LoadMaps(string userMapFile, List<string> userActionFiles)
        {
            /////// Load actions //////////

            // First the builtin actions.
            Type ti = typeof(IEditAction);
            Assembly assy = Assembly.GetAssembly(ti);

            foreach (Type t in assy.GetTypes())
            {
                if (ti.IsAssignableFrom(t) && !t.IsAbstract)
                {
                    //var inst = Activator.CreateInstance(i);
                    // Actions don't have default constructors so use this:
                    var inst = FormatterServices.GetUninitializedObject(t);

                    _actions.Add(t.Name, inst as IEditAction);
                }
            }

            // Now the user actions.
            foreach(string uaf in userActionFiles)
            {
                // TODO1 need to compile source. Add to _actions. Overwrite?

            }

            ///////////// Load mappings /////////////////

            // Load the default control map definitions.
            ControlMap ctrlMap = ControlMap.Load(@".\Resources\ctlmap.default");

            // Load the user control map definitions.
            ControlMap userMap = ControlMap.Load(userMapFile);

            if (userMap != null)
            {
                // Copy/overlay into defMap. TODO1 Duplicates?
                userMap.ControlSpecs.ForEach(cs => ctrlMap.ControlSpecs.Add(cs));
            }
            else
            {
                // TODO1 Error: json load failure.
            }

            ///////////// Bind mappings /////////////////

            // Bind the action implementations to the input configs.
            foreach (ControlSpec asp in ctrlMap.ControlSpecs)
            {
                if(_actions.ContainsKey(asp.ActionName))
                {
                    Action act = new Action
                    {
                        Name = asp.ActionName,
                        UserAction = false,
                        EditAction = _actions[asp.ActionName]
                    };

                    // Key binding?
                    if (asp.Chord1 != Keys.None)
                    {
                        //act.
                        var key = (asp.Chord1, asp.Chord2);
                        if(!_keyActions.ContainsKey(key))
                        {
                            _keyActions.Add(key, act);
                        }
                        else
                        {
                            // TODO1 overwrite - notify user?
                            _keyActions[key] = act;
                        }
                    }

                    // Menu binding? TODO1
                    if (asp.Menu != "")
                    {
                        if (asp.SubMenu != "")
                        {
                        }

                        if (asp.ContextMenu)
                        {
                        }
                    }
                }
                else
                {
                    // TODO1 invalid spec
                }
            }
        }

        public IEditAction GetEditAction(Keys chord1, Keys chord2 = Keys.None)
        {
            var key = (chord1, chord2);

            return _keyActions.ContainsKey(key) ? _keyActions[key].EditAction : null;
        }
    }
}
