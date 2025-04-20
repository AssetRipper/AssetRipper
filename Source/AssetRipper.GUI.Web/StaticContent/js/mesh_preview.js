// Get the canvas element
const canvas = document.getElementById('babylonRenderCanvas');

// Retrieve the GLB file path from the custom attribute
const glbPath = canvas.getAttribute('glb-data-path');

// Create Babylon.js engine
const engine = new BABYLON.Engine(canvas, true);

// Create the scene
const createScene = function () {
	const scene = new BABYLON.Scene(engine);

	// Create a basic light
	const light = new BABYLON.HemisphericLight("light1", new BABYLON.Vector3(0, 1, 0), scene);

	// Create an ArcRotateCamera that rotates around a target position
	const camera = new BABYLON.ArcRotateCamera("Camera", Math.PI / 2, Math.PI / 2, 2, new BABYLON.Vector3(0, 0, 0), scene);
	camera.attachControl(canvas, true);

	// Load the GLB file from the path stored in the custom attribute
	BABYLON.SceneLoader.Append("", glbPath, scene, function (scene) {
		scene.createDefaultCameraOrLight(true, true, true);
		scene.activeCamera.alpha += Math.PI;
	});

	return scene;
};

// Create the scene
const scene = createScene();

// Render the scene
engine.runRenderLoop(function () {
	engine.resize();
	scene.render();
});

// Resize the engine on window resize
window.addEventListener('resize', function () {
	engine.resize();
});
