using PlazmaGames.Core.Events;
using UnityEngine;
using UnityEngine.Events;
using static HTJ21.Events;

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

        // Act1
        public record ArriveAtNeighborhood();
        public static EventResponse NewArriveAtNeighborhood(UnityAction<Component, ArriveAtNeighborhood> func) => new EventResponse((comp, data) => func.Invoke(comp, data as ArriveAtNeighborhood));
        public record ArriveAtHouse();
        public static EventResponse NewArriveAtHouse(UnityAction<Component, ArriveAtHouse> func) => new EventResponse((comp, data) => func.Invoke(comp, data as ArriveAtHouse));

        // Home 1 
        public record Home1FakeScare();
        public static EventResponse NewHome1FakeScare(UnityAction<Component, Home1FakeScare> func) => new EventResponse((comp, data) => func.Invoke(comp, data as Home1FakeScare));
        public record Home1RealScare();
        public static EventResponse NewHome1RealScare(UnityAction<Component, Home1RealScare> func) => new EventResponse((comp, data) => func.Invoke(comp, data as Home1RealScare));
        public record Home1ToggleMusic();
        public static EventResponse NewHome1ToggleMusic(UnityAction<Component, Home1ToggleMusic> func) => new EventResponse((comp, data) => func.Invoke(comp, data as Home1ToggleMusic));

        // Act 2
        public record Act2EnterBathroom();
        public static EventResponse NewAct2EnterBathroom(UnityAction<Component, Act2EnterBathroom> func) => new EventResponse((comp, data) => func.Invoke(comp, data as Act2EnterBathroom));
        public record Act2EnterShower();
        public static EventResponse NewAct2EnterShower(UnityAction<Component, Act2EnterShower> func) => new EventResponse((comp, data) => func.Invoke(comp, data as Act2EnterShower));


        // Epilogue Part 1
        public record VoidNextCheck();
        public static EventResponse NewVoidNextCheck(UnityAction<Component, VoidNextCheck> func) => new EventResponse((comp, data) => func.Invoke(comp, data as VoidNextCheck));
        public record RoadSectionFinished();
        public static EventResponse NewRoadSectionFinished(UnityAction<Component, RoadSectionFinished> func) => new EventResponse((comp, data) => func.Invoke(comp, data as RoadSectionFinished));
        public record RoomSectionStart();
        public static EventResponse NewRoomSectionFinished(UnityAction<Component, RoomSectionFinished> func) => new EventResponse((comp, data) => func.Invoke(comp, data as RoomSectionFinished));
        public record RoomSectionFinished();
        public static EventResponse NewRoomSectionStart(UnityAction<Component, RoomSectionStart> func) => new EventResponse((comp, data) => func.Invoke(comp, data as RoomSectionStart));
        
        public record RevealCult();
        public static EventResponse NewRevealCult(UnityAction<Component, RevealCult> func) => new EventResponse((comp, data) => func.Invoke(comp, data as RevealCult));
        
        public record AtBloodTrail();
        public static EventResponse NewAtBloodTrail(UnityAction<Component, AtBloodTrail> func) => new EventResponse((comp, data) => func.Invoke(comp, data as AtBloodTrail));
        public record CultCutscene();
        public static EventResponse NewCultCutscene(UnityAction<Component, CultCutscene> func) => new EventResponse((comp, data) => func.Invoke(comp, data as CultCutscene));
        public record GotoFinalCarScene();
        public static EventResponse NewGotoFinalCarScene(UnityAction<Component, GotoFinalCarScene> func) => new EventResponse((comp, data) => func.Invoke(comp, data as GotoFinalCarScene));
        public record End();
        public static EventResponse NewEnd(UnityAction<Component,End> func) => new EventResponse((comp, data) => func.Invoke(comp, data as End));
    }
}
