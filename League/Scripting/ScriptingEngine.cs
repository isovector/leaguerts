using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.CSharp;
using System.CodeDom.Compiler;
using System.Reflection;
using System.IO;
using Microsoft.Xna.Framework;
using System.Text.RegularExpressions;
using System.Threading;

namespace LeagueEngine.Scripting {
    /// <summary>
    /// Compiles, hooks and invokes .l scripts
    /// </summary>
    public class ScriptingEngine {
        public delegate void AsyncCall();

        /// <summary>
        /// An instance of the script class for persistance.
        /// </summary>
        public object ScriptInstance;

        /// <summary>
        /// The compiled assembly.
        /// </summary>
        Assembly Assembly;
        public League Engine;

        /// <summary>
        /// A dictionary of event:specific = Methods[]
        /// </summary>
        public Dictionary<string, Dictionary<object, List<MethodInfo>>> Hooks = new Dictionary<string, Dictionary<object, List<MethodInfo>>>();

        /// <summary>
        /// A dictionary of methodname = component
        /// </summary>
        public Dictionary<string, IGameComponent> Components = new Dictionary<string, IGameComponent>();

        /// <summary>
        /// A list of executing threads to ensure they aren't garbage collected while running.
        /// </summary>
        public List<Thread> Threads = new List<Thread>();

        /// <summary>
        /// Compiles map/script.l, common.l and abilities.l
        /// </summary>
        /// <param name="game">The instance of League</param>
        public ScriptingEngine(League game) {
            Engine = game;

            string data = "";
            if (Engine.MpqContent.Archive.FileExists(@"map\script.l")) {
                StreamReader sr = new StreamReader(Engine.MpqContent.Archive.OpenFile(@"map\script.l"));
                data = sr.ReadToEnd();
                sr.Close();

                // Filter out nasty codes
                data = Regex.Replace(data, "System\\.*", "");

                // Creates a #nocommon token which may be added to the bottom of
                // map/script.l to disable common.l being compiled.
                data = data.Replace("#nogame", "/*");
            }

            data += "/**/\r\n" + File.ReadAllText(@"Content\scripts\game.l");
            data += "\r\n" + File.ReadAllText(@"Content\scripts\common.l");
            data += "\r\n" + File.ReadAllText(@"Content\scripts\abilities.l");
            data += "\r\n" + File.ReadAllText(@"Content\scripts\graphics.l");
            data += "\r\n" + File.ReadAllText(@"Content\scripts\resources.l");

            // Bind Script to League.Engine.Script
            data = data.Replace("Script.", "League.Engine.Script.");

            // Turn event and declaration syntax into attributes and public modifiers
            data = Regex.Replace(data, "^on (?<event>[^:]+):", "[${event}] public", RegexOptions.Multiline);
            data = Regex.Replace(data, "^declare (?<event>[^:]+):", "[${event}] public", RegexOptions.Multiline);

            // Create async delegates
            data = Regex.Replace(data, @"\(\s*async\s*\{", "(delegate() {");


            // Compile it good
            CSharpCodeProvider codeprov = new CSharpCodeProvider();
            CompilerParameters cparams = new CompilerParameters();

            cparams.GenerateInMemory = true;
            cparams.ReferencedAssemblies.Add("System.dll");
            cparams.ReferencedAssemblies.Add(@"c:\Program Files\Microsoft XNA\XNA Game Studio\v2.0\References\Windows\x86\Microsoft.Xna.Framework.dll");
            cparams.ReferencedAssemblies.Add(@"c:\Program Files\Microsoft XNA\XNA Game Studio\v2.0\References\Windows\x86\Microsoft.Xna.Framework.Game.dll");
            cparams.ReferencedAssemblies.Add("League.exe");
            cparams.ReferencedAssemblies.Add("System.Windows.Forms.dll");

            string code = "using System; using System.Collections.Generic; using LeagueEngine; using LeagueEngine.Visual; using LeagueEngine.Scripting; using Microsoft.Xna.Framework; using Microsoft.Xna.Framework.Graphics; using Thread = System.Threading.Thread; using ThreadStart = System.Threading.ThreadStart;"
                        + "namespace GameScript { "
                        + "public class CompiledScript { "
                        + data
                        + "} }";

            CompilerResults res = codeprov.CompileAssemblyFromSource(cparams, code);

            foreach (CompilerError error in res.Errors) {
                throw new Exception(error.ToString());
            }

            Assembly = res.CompiledAssembly;
            ScriptInstance = Assembly.CreateInstance("GameScript.CompiledScript");
            MethodInfo[] methods = Assembly.GetType("GameScript.CompiledScript").GetMethods();

            // Look through all methods to hook them
            // NOTE: Events do not need to be manually registered into the Scripting engine
            // as all Attributes are considered Events. However, the only Events which will
            // be invoked are those Attributes defined by League.
            foreach (MethodInfo method in methods) {
                object[] attribs = method.GetCustomAttributes(false);
                foreach (Attribute attrib in attribs) {
                    object specific = attrib is Specific ? (attrib as Specific).Data : "";
                    string hook = attrib.ToString().Replace("Attribute", "").Replace("LeagueEngine.Scripting.", "");

                    // Normal events
                    if (!(attrib is TimeElapsedAttribute || attrib is PeriodicAttribute || attrib is UpdateAttribute)) {
                        Hook(hook, specific, method);
                    } 
                    // Special events which require more precision than Invoke can provide
                    else if (attrib is TimeElapsedAttribute || attrib is PeriodicAttribute || attrib is UpdateAttribute) {
                        TimeEvent te = new TimeEvent(Engine, 
                            method, attrib is UpdateAttribute ? 0 : (float)specific, 
                            attrib is TimeElapsedAttribute ? false : true, 
                            attrib is UpdateAttribute ? false : true);
                        Hook(hook, method, te);
                    }
                }
            }
        }

        /// <summary>
        /// Calls an event in the script
        /// </summary>
        /// <param name="e">The event</param>
        /// <param name="specific">The specific event, or null for none</param>
        /// <param name="data">Arguments for the event</param>
        public void InvokeEvent(string e, object specific, params object[] data) {
            e = e.ToLower();
            if (specific == null) specific = "";
            if (Hooks.ContainsKey(e) && Hooks[e].ContainsKey(specific))
                foreach (MethodInfo method in Hooks[e][specific])
                    if (data.Length == method.GetParameters().Length)
                        StartMethod(method, data);
        }

        /// <summary>
        /// Calls an event in the script and gets data from it. This is run synchronously.
        /// </summary>
        /// <param name="e">The event to be invoked</param>
        /// <param name="specific">The specific event, or null for none</param>
        /// <param name="data">Arguments for the event</param>
        /// <returns>The result of the first matching event</returns>
        public object InvokeSyncEvent(string e, object specific, params object[] data) {
            e = e.ToLower();
            if (specific == null) specific = "";
            if (Hooks.ContainsKey(e) && Hooks[e].ContainsKey(specific))
                foreach (MethodInfo method in Hooks[e][specific])
                    if (data.Length == method.GetParameters().Length)
                        return method.Invoke(ScriptInstance, data);

            return null;
        }

        /// <summary>
        /// Invokes every synchronous event, instead of returning the first.
        /// </summary>
        /// <param name="e">The event to be invoked</param>
        /// <param name="specific">The specific event, or null for none</param>
        /// <param name="data">Arguments for the event</param>
        public void InvokeAllSyncEvent(string e, object specific, params object[] data) {
            e = e.ToLower();
            if (specific == null) specific = "";
            if (Hooks.ContainsKey(e) && Hooks[e].ContainsKey(specific))
                foreach (MethodInfo method in Hooks[e][specific])
                    if (data.Length == method.GetParameters().Length)
                        method.Invoke(ScriptInstance, data);
        }

        /// <summary>
        /// Hooks a method into the scripting engine
        /// </summary>
        /// <param name="e">The event into which to be hooked</param>
        /// <param name="specific">The specific event, or null for none</param>
        /// <param name="name">The method name to be hooked</param>
        public void Hook(string e, object specific, string name) {
            Hook(e, specific, Assembly.GetType("GameScript.CompiledScript").GetMethod(name));
        }

        /// <summary>
        /// Hooks a method into the scripting engine
        /// </summary>
        /// <param name="e">The event into which to be hooked</param>
        /// <param name="specific">The specific event, or null for none</param>
        /// <param name="method">The method to be hooked</param>
        private void Hook(string e, object specific, MethodInfo method) {
            e = e.ToLower();
            if (!Hooks.ContainsKey(e))
                Hooks.Add(e, new Dictionary<object, List<MethodInfo>>());
            if (!Hooks[e].ContainsKey(specific))
                Hooks[e].Add(specific, new List<MethodInfo>());
            Hooks[e][specific].Add(method);
        }

        /// <summary>
        /// Hooks a component (Timing Event) into the scripting engine
        /// </summary>
        /// <param name="e">The event into which to be hooked</param>
        /// <param name="method">The specific event, or null for none</param>
        /// <param name="component">The component to be hooked</param>
        private void Hook(string e, MethodInfo method, IGameComponent component) {
            Components.Add(method.Name, component);
            Engine.Components.Add(component);
        }

        /// <summary>
        /// Unhooks a method from the scripting engine
        /// </summary>
        /// <param name="e">The hooked event</param>
        /// <param name="name">The name of the method to be unhooked</param>
        public void Unhook(string e, string name) {
            if (Hooks.ContainsKey(e)) {
                Dictionary<object, List<MethodInfo>>.Enumerator numer = Hooks[e].GetEnumerator();
                for (int i = 0; i < Hooks[e].Count; i++) {
                    KeyValuePair<object, List<MethodInfo>> pair = numer.Current;
                    for (int j = 0; j < pair.Value.Count; j++)
                        if (pair.Value[i].Name == name) {
                            Hooks[e][pair.Key].RemoveAt(j);
                            goto breakpoint;
                        }
                }
            }

            breakpoint:
            if (Components.ContainsKey(name)) {
                Engine.Components.Remove(Components[name]);
                Components.Remove(name);
            }
        }

        /// <summary>
        /// Starts an asynchronous method
        /// </summary>
        /// <param name="method">The script method to be invoked</param>
        /// <param name="data">Arguments for the function</param>
        public void StartMethod(MethodInfo method, params object[] data) {
            try {
                Thread t = new Thread(new ParameterizedThreadStart(delegate(object o) {
                    method.Invoke(ScriptInstance, data);
                    Threads.Remove((Thread)o);
                }));
                Threads.Add(t);
                t.Start(t);
            } catch (Exception e) {
                File.WriteAllText("error.txt", e.InnerException != null ? e.InnerException.Message : e.Message);
            }
        }

        public void StartCallback(AsyncCall call) {
            Thread t = new Thread(new ParameterizedThreadStart(delegate(object o) {
                call();
                Threads.Remove((Thread)o);
            }));
            Threads.Add(t);
            t.Start(t);
        }

    }
}
