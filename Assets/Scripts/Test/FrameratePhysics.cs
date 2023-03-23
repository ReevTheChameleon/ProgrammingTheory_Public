/*
Rationale:
Sometimes, you have a mix of things that are updated with Physics and with framerate.
Examples of the first are Rigidbody and collision detection, including positional update
of such object, which is usually done via rigidbody.position rather than transform.position,
and the second are Input, shaders, and (usually) Cameras.
Because the first updates every Time.fixedDeltaTime rather than every frame, it will cause
small hiccups even when the framerate is very high. Particularly, in some cases, the
interference pattern is obvious enough to be seen even at high framerate and low timestep.
To get rid of this problem entirely, this script takes Physics simulation into its own hand,
and make sure Physics is simulated every frame while retaining the basic simulation timestep
of Time.fixedDeltaTime. 

Technical Note: Due to Unity's execution sequence, the execution is like this:
FixedUpdate -> Input and InputAction events -> [FrameratePhysics] -> normal Update
What can differ from normal autoSimulation is that Input is processed BEFORE Physics is updated.
*/

using UnityEngine;

namespace Chameleon{

/* Run before other's Update so changes made to Rigidbody are visible to other Updates.
You can call simulateImmediate() if you want to update Physics again in that frame. */
[DefaultExecutionOrder(-2)]
public class FrameratePhysics : LoneMonoBehaviour<FrameratePhysics>{
	public float minTimestep = 0.005f; //200 fps, just to limit amount of Update in Editor
	private double prevTime;
	private float elapsedTime = 0.0f;

	void OnEnable(){
		Physics.autoSimulation = false;
		prevTime = Time.timeAsDouble;
		elapsedTime = 0.0f;
	}
	void OnDisable(){
		Physics.autoSimulation = true;
	}
	/* Note: Unity already ensure that difference between each Time.time call are
	LESS than Time.maximumDeltaTime, which corresponds to "Maximum Allowed Timestep"
	in the Project Settings. Hence, we do not check or clamp that. */
	/* Note2: Previously I also split some work into FixedUpdate, thinking that
	it will alleviate workload done in frame if framerate is low. I discovered later
	that FixedUpdate is not called independently of frame, but is called repeatedly
	WITHIN that frame until Time.fixedTime catches up with Time.time. Hence, split the work
	to FixedUpdate does not help anything. */
	void Update(){
		elapsedTime += (float)(Time.timeAsDouble-prevTime);
		if(elapsedTime < minTimestep){
			return;}
		simulateImmediate();
	}
	public void simulateImmediate(){
		float fixedDeltaTime = Time.fixedDeltaTime;
		while(elapsedTime > fixedDeltaTime){
			Physics.Simulate(fixedDeltaTime);
			elapsedTime -= fixedDeltaTime;
		}
		/* This makes sure to simulate once even when elapsedTime is zero
		in case user update Rigidbody directly and requests simulation immediately. */
		Physics.Simulate(Mathf.Max(0.0f,elapsedTime));
		elapsedTime = 0.0f;
		prevTime = Time.timeAsDouble;
	}
	//void FixedUpdate(){
	//	/* Calling Time.timeAsDouble in FixedUpdate will return Time.fixedTimeAsDouble anyway */
	//	elapsedTime += (float)(Time.fixedTimeAsDouble-prevTime);
	//	if(elapsedTime < minTimestep){
	//		return;}
	//	float fixedDeltaTime = Time.fixedDeltaTime;
	//	while(elapsedTime > fixedDeltaTime){
	//		Physics.Simulate(fixedDeltaTime);
	//		elapsedTime -= fixedDeltaTime;
	//	}
	//	prevTime = Time.fixedTimeAsDouble;
	//}
}

} //end namespace Chameleon
