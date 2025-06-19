using PlazmaGames.Core.Events;
using UnityEngine;
using UnityEngine.Events;

namespace HTJ21
{
    public static class Events
    {
        public record StartGame();
        public static EventResponse NewStartGame(UnityAction<Component, StartGame> func) => new EventResponse((comp, data) => func.Invoke(comp, data as StartGame));
        public record HeadlightTutorial();
        public static EventResponse NewHeadlightTutorial(UnityAction<Component, HeadlightTutorial> func) => new EventResponse((comp, data) => func.Invoke(comp, data as HeadlightTutorial));
        public record DreamFallenLog();
        public static EventResponse NewDreamFallenLog(UnityAction<Component, DreamFallenLog> func) => new EventResponse((comp, data) => func.Invoke(comp, data as DreamFallenLog));
        public record DreamSighting();
        public static EventResponse NewDreamSighting(UnityAction<Component, DreamSighting> func) => new EventResponse((comp, data) => func.Invoke(comp, data as DreamSighting));
        public record DreamFoundCult();
        public static EventResponse NewDreamFoundCult(UnityAction<Component, DreamFoundCult> func) => new EventResponse((comp, data) => func.Invoke(comp, data as DreamFoundCult));
        public record DreamUnderMoon();
        public static EventResponse NewDreamUnderMoon(UnityAction<Component, DreamUnderMoon> func) => new EventResponse((comp, data) => func.Invoke(comp, data as DreamUnderMoon));
        public record TreeFall();
        public static EventResponse NewTreeFall(UnityAction<Component, TreeFall> func) => new EventResponse((comp, data) => func.Invoke(comp, data as TreeFall));
        public record TreeReroute1();
        public static EventResponse NewTreeReroute1(UnityAction<Component, TreeReroute1> func) => new EventResponse((comp, data) => func.Invoke(comp, data as TreeReroute1));
        public record TreeReroute2();
        public static EventResponse NewTreeReroute2(UnityAction<Component, TreeReroute2> func) => new EventResponse((comp, data) => func.Invoke(comp, data as TreeReroute2));

        // Prologue
        public record PrologueLookAtMoon();
        public static EventResponse NewPrologueLookAtMoon(UnityAction<Component, PrologueLookAtMoon> func) => new EventResponse((comp, data) => func.Invoke(comp, data as PrologueLookAtMoon));

    }
}
