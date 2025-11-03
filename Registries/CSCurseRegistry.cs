using CursedScraps.Behaviours.Curses;
using CursedScraps.Managers;
using CursedScraps.Patches;
using GameNetcodeStuff;
using LegaFusionCore.Behaviours.Shaders;
using LegaFusionCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CursedScraps.Registries;

public class CSCurseRegistry : MonoBehaviour
{
    public class CurseEffectType
    {
        public readonly string Name;
        public readonly float Multiplier;
        public readonly string Weight;
        public readonly int Duration;

        public CurseEffectType(string name, float multiplier, string weight, int duration)
        {
            Name = name;
            Multiplier = multiplier;
            Weight = weight;
            Duration = duration;
        }
    }

    public static List<CurseEffectType> curseEffectTypes = [];

    public static void RegisterCurse(string name, float multiplier, string weight, int duration)
        => curseEffectTypes.Add(new CurseEffectType(name, multiplier, weight, duration));

    public abstract class CurseEffect
    {
        public CurseEffectType EffectType { get; }
        public int PlayerWhoHit { get; }
        public int Duration { get; }
        public float EndTime { get; private set; }

        public Action OnApply;
        public Action OnExpire;
        public Action OnUpdate;

        public float RemainingTime => EndTime - Time.time;

        protected CurseEffect(CurseEffectType effectType, int playerWhoHit, int duration, Action onApply, Action onExpire, Action onUpdate)
        {
            EffectType = effectType;
            PlayerWhoHit = playerWhoHit;
            Duration = duration;
            EndTime = Time.time + duration;
            OnApply = onApply;
            OnExpire = onExpire;
            OnUpdate = onUpdate;
        }

        public virtual void Apply(GameObject entity)
        {
            OnApply?.Invoke();

            PlayerControllerB player = LFCUtilities.GetSafeComponent<PlayerControllerB>(entity);
            if (LFCUtilities.ShouldNotBeLocalPlayer(player))
            {
                if (HasCurse(entity, Constants.DIMINUTIVE))
                    CustomPassManager.RemoveAuraFromObjects([entity.gameObject], $"{CursedScraps.modName}{CursedScraps.cursedShader.name}");
                else
                    CustomPassManager.SetupAuraForObjects([entity.gameObject], CursedScraps.cursedShader, $"{CursedScraps.modName}{CursedScraps.cursedShader.name}");
            }
        }

        public virtual void Update(GameObject entity) => OnUpdate?.Invoke();

        public virtual void Expire(GameObject entity)
        {
            OnExpire?.Invoke();
            CustomPassManager.RemoveAuraFromObjects([entity.gameObject], $"{CursedScraps.modName}{CursedScraps.cursedShader.name}");
        }

        public bool IsExpired() => Time.time >= EndTime;
    }

    private static readonly Dictionary<GameObject, Dictionary<CurseEffectType, CurseEffect>> activeCurses = [];

    private static CSCurseRegistry _instance;
    public static CSCurseRegistry Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject curseEffect = new GameObject("CurseEffect");
                _instance = curseEffect.AddComponent<CSCurseRegistry>();
                DontDestroyOnLoad(curseEffect);
            }
            return _instance;
        }
    }

    private void Update()
    {
        List<(GameObject, CurseEffectType)> expiredCurses = [];
        List<GameObject> deadEntities = [];

        foreach (KeyValuePair<GameObject, Dictionary<CurseEffectType, CurseEffect>> kv in activeCurses)
        {
            GameObject entity = kv.Key;
            if (entity == null)
            {
                deadEntities.Add(entity);
                continue;
            }
            Dictionary<CurseEffectType, CurseEffect> effects = kv.Value;

            foreach (KeyValuePair<CurseEffectType, CurseEffect> effectKvp in effects)
            {
                CurseEffect effect = effectKvp.Value;
                effect.Update(entity);
                if (effect.Duration != -1 && effect.IsExpired()) expiredCurses.Add((entity, effectKvp.Key));
            }
        }

        deadEntities.ForEach(e => activeCurses.Remove(e));
        expiredCurses.ForEach(e => RemoveCurse(e.Item1, e.Item2));
    }

    public static void ApplyCurse(GameObject entity, string name, int playerWhoHit, int duration = -1, Action onApply = null, Action onExpire = null, Action onUpdate = null)
    {
        CurseEffect curse = name switch
        {
            Constants.BLURRY => new Blurry(playerWhoHit, duration, onApply, onExpire, onUpdate),
            Constants.CAPTIVE => new Captive(playerWhoHit, duration, onApply, onExpire, onUpdate),
            Constants.CONFUSION => new Confusion(playerWhoHit, duration, onApply, onExpire, onUpdate),
            Constants.DEAFNESS => new Deafness(playerWhoHit, duration, onApply, onExpire, onUpdate),
            Constants.DIMINUTIVE => new Diminutive(playerWhoHit, duration, onApply, onExpire, onUpdate),
            Constants.ERRANT => new Errant(playerWhoHit, duration, onApply, onExpire, onUpdate),
            Constants.EXPLORATION => new Exploration(playerWhoHit, duration, onApply, onExpire, onUpdate),
            Constants.FRAGILE => new Fragile(playerWhoHit, duration, onApply, onExpire, onUpdate),
            Constants.INHIBITION => new Inhibition(playerWhoHit, duration, onApply, onExpire, onUpdate),
            Constants.MUTE => new Mute(playerWhoHit, duration, onApply, onExpire, onUpdate),
            Constants.ONE_FOR_ALL => new OneForAll(playerWhoHit, duration, onApply, onExpire, onUpdate),
            Constants.PARALYSIS => new Paralysis(playerWhoHit, duration, onApply, onExpire, onUpdate),
            Constants.SACRIFICE => new Sacrifice(playerWhoHit, duration, onApply, onExpire, onUpdate),
            Constants.SHADOW => new Shadow(playerWhoHit, duration, onApply, onExpire, onUpdate),
            _ => null
        };

        if (curse != null) ApplyCurse(entity, curse);
    }

    public static void ApplyCurse(GameObject entity, CurseEffect curse)
    {
        _ = Instance;
        if (!activeCurses.ContainsKey(entity)) activeCurses[entity] = [];

        Dictionary<CurseEffectType, CurseEffect> curses = activeCurses[entity];

        if (curses.TryGetValue(curse.EffectType, out CurseEffect existingCurses))
        {
            // Remplace seulement si la nouvelle durée est plus longue
            if (curse.RemainingTime > existingCurses.RemainingTime)
            {
                existingCurses.Expire(entity);
                curses[curse.EffectType] = curse;
                curse.Apply(entity);
            }
        }
        else
        {
            curses[curse.EffectType] = curse;
            curse.Apply(entity);
        }

        PlayerControllerB player = LFCUtilities.GetSafeComponent<PlayerControllerB>(entity);
        if (player == null) return;

        if (LFCUtilities.ShouldBeLocalPlayer(player))
        {
            if (ConfigManager.isCurseInfoOn.Value)
                HUDManager.Instance.DisplayTip(Constants.IMPORTANT_INFORMATION, $"You have just been affected by the curse {curse.EffectType.Name}");
        }
        HUDManagerPatch.RefreshCursesText(player);
    }

    public static void RemoveCurse(GameObject entity, string name)
    {
        CurseEffectType type = curseEffectTypes.FirstOrDefault(t => t.Name.Equals(name));
        if (type != null) RemoveCurse(entity, type);
    }

    public static void RemoveCurse(GameObject entity, CurseEffectType curseType)
    {
        if (activeCurses.TryGetValue(entity, out Dictionary<CurseEffectType, CurseEffect> curses) && curses.TryGetValue(curseType, out CurseEffect curse))
        {
            curse.Expire(entity);
            _ = curses.Remove(curseType);
            HUDManagerPatch.RefreshCursesText(LFCUtilities.GetSafeComponent<PlayerControllerB>(entity));
        }
    }

    public static List<CurseEffectType> GetCurses(GameObject entity) => !activeCurses.TryGetValue(entity, out Dictionary<CurseEffectType, CurseEffect> curses) ? ([]) : [.. curses.Keys];
    public static bool HasCurse(GameObject entity) => activeCurses.TryGetValue(entity, out Dictionary<CurseEffectType, CurseEffect> _);
    public static bool HasCurse(GameObject entity, string name)
        => activeCurses.TryGetValue(entity, out Dictionary<CurseEffectType, CurseEffect> curses)
        && curseEffectTypes.FirstOrDefault(t => t.Name == name) is { } type
        && curses.ContainsKey(type);

    public static void ClearCurses(GameObject entity)
    {
        if (!activeCurses.TryGetValue(entity, out Dictionary<CurseEffectType, CurseEffect> curses)) return;

        curses.Keys.ToList().ForEach(e => RemoveCurse(entity, e));
        _ = activeCurses.Remove(entity);
    }

    public static void ClearCurses() => activeCurses.Keys.ToList().ForEach(ClearCurses);
}
