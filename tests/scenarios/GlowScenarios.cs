using System.Collections;
using System.Collections.Generic;
using System.Linq;
using RoR2;
using Ror2Mods.GateProbe;
using UnityEngine;
using UnityEngine.Rendering;

// In-game scenarios run by the boot gate in a real run (host, no local user). Rendering is not observed in batch mode:
// these check which renderers carry the glow material, its depth test, and that vanilla highlights are untouched.
namespace Ror2Mods.InteractableHighlight.Scenarios
{
    public static class GlowScenarios
    {
        private const string Chest = "RoR2/Base/Chest1/iscChest1.asset";
        private const string MultiShop = "RoR2/Base/TripleShop/iscTripleShop.asset";

        private static List<Renderer> RenderersOf(GameObject target)
        {
            var highlight = target.GetComponent<Highlight>();
            if (!highlight) return new List<Renderer>();
            if (highlight.targetRenderer) return new List<Renderer> { highlight.targetRenderer };
            return (highlight.GetTargetRendererList() ?? new List<Renderer>()).Where(r => r).ToList();
        }

        private static Material GlowOf(Renderer renderer)
        {
            var materials = renderer.sharedMaterials;
            var last = materials.Length > 0 ? materials[materials.Length - 1] : null;
            return last && last.name.StartsWith("Ror2ModsGlow") ? last : null;
        }

        private static bool Glows(GameObject target, string category)
        {
            var renderers = RenderersOf(target);
            return renderers.Count > 0 && renderers.All(r => GlowOf(r) && GlowOf(r).name == "Ror2ModsGlow" + category);
        }

        private static bool Dark(GameObject target) => RenderersOf(target).All(r => !GlowOf(r));

        [Scenario("chests and multishop terminals glow in their category, depth-tested")]
        public static IEnumerator ChestAndShopGlow(ScenarioContext ctx)
        {
            var chest = ctx.SpawnInteractable(Chest);
            ctx.SpawnInteractable(MultiShop);
            yield return ctx.WaitUntil(() => Glows(chest, "Chest"), 5f);
            ctx.Assert(Glows(chest, "Chest"), "the spawned chest does not carry the Chest glow");
            var terminals = Object.FindObjectsOfType<ShopTerminalBehavior>().Where(t => t.name.StartsWith("MultiShopTerminal") && !t.NetworkhasBeenPurchased).ToList();
            yield return ctx.WaitUntil(() => terminals.All(t => Glows(t.gameObject, "Shop")), 5f);
            ctx.Assert(terminals.Count >= 3, $"expected the multishop's 3 terminals, found {terminals.Count}");
            ctx.Assert(terminals.All(t => Glows(t.gameObject, "Shop")), "not every multishop terminal carries the Shop glow");
            var glow = GlowOf(RenderersOf(chest)[0]);
            ctx.Assert(glow.GetFloat("_ZTest") == (float)CompareFunction.LessEqual, $"glow depth test is {glow.GetFloat("_ZTest")}, so it could show through walls");
        }

        [Scenario("an opened chest stops glowing")]
        public static IEnumerator OpenedChestIsDark(ScenarioContext ctx)
        {
            var chest = ctx.SpawnInteractable(Chest);
            yield return ctx.WaitUntil(() => Glows(chest, "Chest"), 5f);
            ctx.Assert(Glows(chest, "Chest"), "the chest never started glowing");
            var counts = RenderersOf(chest).ToDictionary(r => r, r => r.sharedMaterials.Length);
            chest.GetComponent<PurchaseInteraction>().SetAvailable(false);
            yield return ctx.WaitUntil(() => Dark(chest), 5f);
            ctx.Assert(Dark(chest), "the used chest still glows");
            ctx.Assert(counts.All(p => p.Key.sharedMaterials.Length == p.Value - 1), "removing the glow did not restore the renderer's own materials");
        }

        [Scenario("the vanilla targeting highlight is never touched")]
        public static IEnumerator VanillaHighlightUntouched(ScenarioContext ctx)
        {
            var chest = ctx.SpawnInteractable(Chest);
            yield return ctx.WaitUntil(() => Glows(chest, "Chest"), 5f);
            var before = Highlight.readonlyHighlightList.Where(h => h).ToDictionary(h => h, h => h.isOn);
            var highlight = chest.GetComponent<Highlight>();
            highlight.isOn = true; // what the game does for the interactable the player aims at
            yield return ctx.Wait(1.5f);
            ctx.Assert(highlight.isOn, "the mod turned the vanilla targeting outline off");
            ctx.Assert(Glows(chest, "Chest"), "the glow disappeared while the vanilla outline was on");
            var changed = before.Where(p => p.Key && p.Key != highlight && p.Key.isOn != p.Value).Select(p => p.Key.name).ToList();
            ctx.Assert(changed.Count == 0, $"vanilla highlight state changed on {string.Join(", ", changed)}");
            highlight.isOn = false;
        }
    }
}
