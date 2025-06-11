using PlazmaGames.Core.Events;
using UnityEngine;
using UnityEngine.Events;

namespace HTJ21
{
    public static class Events
    {
        public record StartGame();
        public static EventResponse NewStartGame(UnityAction<Component, StartGame> func) => new EventResponse((comp, data) => func.Invoke(comp, data as StartGame));
        public record DreamFallenLog();
        public static EventResponse NewDreamFallenLog(UnityAction<Component, DreamFallenLog> func) => new EventResponse((comp, data) => func.Invoke(comp, data as DreamFallenLog));
        public record DreamSighting();
        public static EventResponse NewDreamSighting(UnityAction<Component, DreamSighting> func) => new EventResponse((comp, data) => func.Invoke(comp, data as DreamSighting));
    }
}
