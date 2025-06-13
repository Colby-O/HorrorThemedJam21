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
    }
}
