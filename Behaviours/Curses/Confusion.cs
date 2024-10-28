using Newtonsoft.Json.Linq;
using System.Linq;
using UnityEngine.InputSystem;

namespace CursedScraps.Behaviours.Curses
{
    public class Confusion
    {
        public static void ApplyConfusion(bool enable)
        {
            if (enable)
            {
                string moveUpKeyboard = null;
                string moveUpGamepad = null;
                string moveDownKeyboard = null;
                string moveDownGamepad = null;
                string moveLeftKeyboard = null;
                string moveLeftGamepad = null;
                string moveRightKeyboard = null;
                string moveRightGamepad = null;
                string jumpKeyboard = null;
                string jumpGamepad = null;
                string crouchKeyboard = null;
                string crouchGamepad = null;

                if (IngamePlayerSettings.Instance.settings != null && !string.IsNullOrEmpty(IngamePlayerSettings.Instance.settings.keyBindings))
                {
                    JObject jsonObject = JObject.Parse(IngamePlayerSettings.Instance.settings.keyBindings);
                    JArray bindings = (JArray)jsonObject?["bindings"];
                    if (bindings != null)
                    {
                        var filteredBindings = bindings.Where(b => ((string)b["action"]).Equals("Movement/Jump") || ((string)b["action"]).Equals("Movement/Crouch") || ((string)b["action"]).Equals("Movement/Move"));

                        foreach (JObject binding in filteredBindings.Cast<JObject>())
                        {
                            string action = (string)binding["action"];
                            string path = (string)binding["path"];

                            if (action.Equals("Movement/Move"))
                            {
                                // Les id ne semblent pas changer d'une personne à l'autre
                                string id = (string)binding["id"];
                                if (id.Equals("ae3df94a-dcc6-4177-b026-0938f8413a45") && (path.StartsWith("<Keyboard>") || path.StartsWith("<Mouse>"))) moveUpKeyboard = path;
                                else if (id.Equals("bb6e2ec9-7f02-4c1e-b136-f45218f65d48") && path.StartsWith("<Gamepad>")) moveUpGamepad = path;
                                else if (id.Equals("bc252037-120e-4c64-9671-40d365f856b3") && (path.StartsWith("<Keyboard>") || path.StartsWith("<Mouse>"))) moveDownKeyboard = path;
                                else if (id.Equals("717338d1-0b10-457a-a2ef-9b43135cbad6") && path.StartsWith("<Gamepad>")) moveDownGamepad = path;
                                else if (id.Equals("ad96b1ce-c0f3-4913-be03-acf077c11064") && (path.StartsWith("<Keyboard>") || path.StartsWith("<Mouse>"))) moveLeftKeyboard = path;
                                else if (id.Equals("1ffdc730-e9c8-4a8a-934f-98fa19bdca4d") && path.StartsWith("<Gamepad>")) moveLeftGamepad = path;
                                else if (id.Equals("756f3db2-a6e6-42d7-9580-70d42154cd11") && (path.StartsWith("<Keyboard>") || path.StartsWith("<Mouse>"))) moveRightKeyboard = path;
                                else if (id.Equals("bc814a6c-e505-4ad6-988b-8e9ce3311a28") && path.StartsWith("<Gamepad>")) moveRightGamepad = path;
                            }
                            else if (action.Equals("Movement/Jump"))
                            {
                                if (path.StartsWith("<Keyboard>") || path.StartsWith("<Mouse>")) jumpKeyboard = path;
                                else if (path.StartsWith("<Gamepad>")) jumpGamepad = path;
                            }
                            else if (action.Equals("Movement/Crouch"))
                            {
                                if (path.StartsWith("<Keyboard>") || path.StartsWith("<Mouse>")) crouchKeyboard = path;
                                else if (path.StartsWith("<Gamepad>")) crouchGamepad = path;
                            }
                        }
                    }
                }

                InputAction moveAction = IngamePlayerSettings.Instance.playerInput.actions.FindAction("Move");

                // Clavier
                if (!string.IsNullOrEmpty(moveDownKeyboard)) moveAction.ApplyBindingOverride(1, new InputBinding { overridePath = moveDownKeyboard });
                else moveAction.ApplyBindingOverride(1, new InputBinding { overridePath = moveAction.bindings[2].path });

                if (!string.IsNullOrEmpty(moveUpKeyboard)) moveAction.ApplyBindingOverride(2, new InputBinding { overridePath = moveUpKeyboard });
                else moveAction.ApplyBindingOverride(2, new InputBinding { overridePath = moveAction.bindings[1].path });

                if (!string.IsNullOrEmpty(moveRightKeyboard)) moveAction.ApplyBindingOverride(3, new InputBinding { overridePath = moveRightKeyboard });
                else moveAction.ApplyBindingOverride(3, new InputBinding { overridePath = moveAction.bindings[4].path });

                if (!string.IsNullOrEmpty(moveLeftKeyboard)) moveAction.ApplyBindingOverride(4, new InputBinding { overridePath = moveLeftKeyboard });
                else moveAction.ApplyBindingOverride(4, new InputBinding { overridePath = moveAction.bindings[3].path });

                // Manette
                if (!string.IsNullOrEmpty(moveDownGamepad)) moveAction.ApplyBindingOverride(6, new InputBinding { overridePath = moveDownGamepad });
                else moveAction.ApplyBindingOverride(6, new InputBinding { overridePath = moveAction.bindings[7].path });

                if (!string.IsNullOrEmpty(moveUpGamepad)) moveAction.ApplyBindingOverride(7, new InputBinding { overridePath = moveUpGamepad });
                else moveAction.ApplyBindingOverride(7, new InputBinding { overridePath = moveAction.bindings[6].path });

                if (!string.IsNullOrEmpty(moveRightGamepad)) moveAction.ApplyBindingOverride(8, new InputBinding { overridePath = moveRightGamepad });
                else moveAction.ApplyBindingOverride(8, new InputBinding { overridePath = moveAction.bindings[9].path });

                if (!string.IsNullOrEmpty(moveLeftGamepad)) moveAction.ApplyBindingOverride(9, new InputBinding { overridePath = moveLeftGamepad });
                else moveAction.ApplyBindingOverride(9, new InputBinding { overridePath = moveAction.bindings[8].path });

                InputAction jumpAction = IngamePlayerSettings.Instance.playerInput.actions.FindAction("Jump");
                InputAction crouchAction = IngamePlayerSettings.Instance.playerInput.actions.FindAction("Crouch");

                // Clavier
                if (!string.IsNullOrEmpty(crouchKeyboard)) jumpAction.ApplyBindingOverride(0, new InputBinding { overridePath = crouchKeyboard });
                else jumpAction.ApplyBindingOverride(0, new InputBinding { overridePath = crouchAction.bindings[0].path });

                if (!string.IsNullOrEmpty(jumpKeyboard)) crouchAction.ApplyBindingOverride(0, new InputBinding { overridePath = jumpKeyboard });
                else crouchAction.ApplyBindingOverride(0, new InputBinding { overridePath = jumpAction.bindings[0].path });

                // Manette
                if (!string.IsNullOrEmpty(crouchGamepad)) jumpAction.ApplyBindingOverride(1, new InputBinding { overridePath = crouchGamepad });
                else jumpAction.ApplyBindingOverride(1, new InputBinding { overridePath = crouchAction.bindings[1].path });

                if (!string.IsNullOrEmpty(jumpGamepad)) crouchAction.ApplyBindingOverride(1, new InputBinding { overridePath = jumpGamepad });
                else crouchAction.ApplyBindingOverride(1, new InputBinding { overridePath = jumpAction.bindings[1].path });
            }
            else
            {
                IngamePlayerSettings.Instance.playerInput.actions.RemoveAllBindingOverrides();
                IngamePlayerSettings.Instance.LoadSettingsFromPrefs();
            }
        }
    }
}
