var EngineSoundLibrary = {
    $node: null,
    $gainNode: null,
    JsEngineSoundInit: function (settings, sampleRate) {
        const AudioContext = window.AudioContext || window.webkitAudioContext;
        const context = new AudioContext({sampleRate});
        const settingsString = UTF8ToString(settings);
        context.audioWorklet.addModule("StreamingAssets/EngineSoundGenerator.js").then(() => {
            console.log("settings: " + settingsString);
            node = new AudioWorkletNode(context, "EngineSoundGenerator", {
                processorOptions: JSON.parse(settingsString)
            });
            gainNode = context.createGain();
            gainNode.gain.value = 1;
            node.connect(gainNode);
            gainNode.connect(context.destination);
        });
    },
    JsEngineSoundSetRpmAndThrottle: function (rpm, throttle) {
        node.port.postMessage({rpm, throttle});
    },
    JsEngineSoundSetVolume: function (volume) {
        gainNode.gain.value = volume;
    },
};

autoAddDeps(EngineSoundLibrary, '$node');
autoAddDeps(EngineSoundLibrary, '$gainNode');
mergeInto(LibraryManager.library, EngineSoundLibrary);
