using HarmonyLib; // HarmonyLib comes included with a NeosModLoader install
using NeosModLoader;
using FrooxEngine;
using FrooxEngine.UIX;

namespace FunnyNeosInspector
{
    public class FunnyNeosInspector : NeosMod
    {
        public override string Name => "FunnyNeosInspector";
        public override string Author => "DeltaNeverUsed";
        public override string Version => "1.0.0";
        //public override string Link => "Haha Don't have one"; // this line is optional and can be omitted

        //private static bool _first_trigger = false;

        public override void OnEngineInit()
        {
            Harmony harmony = new Harmony("FunnyNeosInspector_Cool");
            harmony.PatchAll();
            // do whatever LibHarmony patching you need
            
            Msg("Inspector Description mod loaded");
        }

        [HarmonyPatch(typeof(InspectorPanel))]
        private static class FunnyInspectorPatch
        {
            [HarmonyPatch("Setup")]
            [HarmonyPostfix]
            public static void addVariableSpace(InspectorPanel __instance)
            {
                var slot = __instance.Slot;
                var space = slot.GetComponentOrAttach<DynamicVariableSpace>();
                space.SpaceName.Value = "InspectorToolTip";
                space.OnlyDirectBinding.Value = true;
                
                Msg($"VariableSpace slot: {slot.Name}, parent: {slot.Parent.Name}");
            }
        }

        [HarmonyPatch(typeof(WorkerInspector))]
        private static class FunnyWorkerInspectorPatch
        {
            [HarmonyPatch("BuildUIForComponent")]
            [HarmonyPostfix]
            public static void componentAdder(WorkerInspector __instance)
            {
                var slot = __instance.Slot;

                for (int slotComponent = 1; slotComponent < slot.ChildrenCount; slotComponent++)
                {
                    var child = slot[slotComponent];

                    var temp_comp = child[0].GetAllChildren()[0].GetComponentInChildren<Text>();
                    if (temp_comp == null)
                        continue;
                    var componentName = temp_comp.Content.Value;
                    
                    for (int fields = 1; fields < child.ChildrenCount; fields++) // Create extra components on thing
                    {
                        var componentComponents = child[fields];
                        var localSlot = componentComponents.FindInChildren("Text");
                        if (localSlot == null || localSlot.GetComponent<ValueField<string>>() != null)
                            continue;
                        
                        temp_comp = localSlot.GetComponent<Text>();
                        var tempButton = localSlot.GetComponent<Button>();
                        if (temp_comp == null || tempButton == null)
                            continue;
                        var fieldName = temp_comp.Content.Value;

                        var valueField = localSlot.AttachComponent<ValueField<string>>();
                        var valueCopyComp = localSlot.AttachComponent<ValueCopy<string>>();
                        
                        var valueCopyEnabler = localSlot.AttachComponent<ValueCopy<bool>>();
                        var BooleanValueDriver = localSlot.AttachComponent<BooleanValueDriver<int>>();

                        var dValue = localSlot.AttachComponent<DynamicValueVariable<string>>();
                        dValue.VariableName.Value = "InspectorToolTip/Tooltip";
                        
                        var dValueRefChanger = localSlot.AttachComponent<ReferenceMultiplexer<IField<string>>>();
                        dValueRefChanger.Target.Target = valueCopyComp.Target;
                        dValueRefChanger.References.Add(null);
                        dValueRefChanger.References.Add(dValue.Value);

                        BooleanValueDriver.TargetField.Value = dValueRefChanger.Index.ReferenceID;
                        BooleanValueDriver.FalseValue.Value = 0;
                        BooleanValueDriver.TrueValue.Value = 1;

                        valueCopyEnabler.Source.Value = tempButton.IsHovering.ReferenceID;
                        valueCopyEnabler.Target.Value = BooleanValueDriver.State.ReferenceID;

                        valueCopyComp.Source.Value = valueField.Value.ReferenceID;

                        var key = $"{componentName}/{fieldName}".Replace("<b>", "").Replace("</b>", "").Replace(":", "");
                        if (DescDict.Dict.TryGetValue(key, out string bla))
                            valueField.Value.Value = fieldName+"\n"+bla;
                    }
                }
            }
        }
    }
}