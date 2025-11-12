var MonouGameScraperLib = {
	$SharedData: {
		gameKey: '',
		waiting: false,
		secure: false,
		generateSignature: async function (secret, bodyStr) {
		  var encoder = new TextEncoder();
		  var key = await crypto.subtle.importKey(
		    "raw",encoder.encode(secret),
		    { name: "HMAC", hash: "SHA-256" },false,["sign"]
		   );
		  var sig = await crypto.subtle.sign("HMAC", key, encoder.encode(bodyStr));
		  return Array.from(new Uint8Array(sig)).map(function(b){ return b.toString(16).padStart(2, "0") }).join("");
		},
		send: async function(gameKey, type, val, generateSignature){
			var data = {key:gameKey, type:type, val:val};
			var dataJSON = JSON.stringify(data);
			var message = {hash:await generateSignature(gameKey, dataJSON), data:dataJSON};
			window.parent.postMessage(
				JSON.stringify(message),
				//"https://monou.gg/"
			);
		}
	},
	MonouGameScraper_Init: function(kPointer){
		SharedData.gameKey = UTF8ToString(kPointer);
		SharedData.send(SharedData.gameKey, "init", false, SharedData.generateSignature);
		window.addEventListener('message', async function(event) {
			var data = JSON.parse(event.data.data || '');
			if(event.data.hash == "test"){ SharedData.waiting = data; SharedData.secure=false; return; }
			if(event.data.hash != await SharedData.generateSignature(SharedData.gameKey, event.data.data)) return;
			switch(data.type){ case "ad": case "adReward": case "sell": SharedData.waiting = data; SharedData.secure=true; break; }
		});
	},
	MonouGameScraper_Start: function(){ SharedData.send(SharedData.gameKey, "start", false, SharedData.generateSignature) },
	MonouGameScraper_Finish: function(score){ SharedData.send(SharedData.gameKey, "finish", score, SharedData.generateSignature) },
	MonouGameScraper_Advance: function(delta){ SharedData.send(SharedData.gameKey, "advance", delta, SharedData.generateSignature) },
	MonouGameScraper_Advertise: function(taskId){
		SharedData.send(SharedData.gameKey, "ad", false, SharedData.generateSignature);
		SharedData.waiting = false;
		var interval = setInterval(function(){
			if(!SharedData.waiting) return;
			clearInterval(interval);
			//resolve(SharedData.waiting.success);
			SendMessage('MonouGameScraper', 'WorkAsyncResult', taskId+"|0");
		},100);
	},
	MonouGameScraper_AdvertiseRewarded: function(taskId){
		SharedData.send(SharedData.gameKey, "adReward", false, SharedData.generateSignature);
		SharedData.waiting = false;
		var interval = setInterval(function(){
			if(!SharedData.waiting) return;
			clearInterval(interval);
			var v = SharedData.waiting && SharedData.waiting.success && 1 || 0;
			SendMessage('MonouGameScraper', 'WorkAsyncResult', taskId+"|"+v);
		},100);
	},
	MonouGameScraper_Sell: function(amount, taskId){
		SharedData.send(SharedData.gameKey, "sell", amount, SharedData.generateSignature);
		SharedData.waiting = false;
		var interval = setInterval(function(){
			if(!SharedData.waiting) return;
			clearInterval(interval);
			var v = SharedData.waiting && SharedData.waiting.success && 1 || 0;
			SendMessage('MonouGameScraper', 'WorkAsyncResult', taskId+"|"+v);
		},100);
	}
};
autoAddDeps(MonouGameScraperLib, '$SharedData');
mergeInto(LibraryManager.library, MonouGameScraperLib);