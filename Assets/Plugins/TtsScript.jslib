mergeInto(LibraryManager.library, {
    Speak: function (strPointer, strPointer2) {
        var str = Pointer_stringify(strPointer);
		var agent = Pointer_stringify(strPointer2);
		console.log("Msg: " + str);
		console.log("Agent: " + agent);
        var msg = new SpeechSynthesisUtterance(str);
        msg.lang = 'en-US';
        msg.volume = 1; // 0 to 1
        msg.rate = 1; // 0.1 to 10
        msg.pitch = 1.5; //0 to 2
        // stop any TTS that may still be active
        window.speechSynthesis.cancel();

		var voices = window.speechSynthesis.getVoices();

		voices.forEach(function(nome, i) {
			//console.log('[forEach]', nome.name, i);
		})

		if(agent == "Bella"){
			msg.voice = voices.filter(function(voice) { return voice.name.includes('Zira'); })[0];
		}

        window.speechSynthesis.speak(msg);
    }
});