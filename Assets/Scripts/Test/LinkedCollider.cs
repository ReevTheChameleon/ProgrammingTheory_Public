using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class LinkedCollider : MonoBehaviour{
	[SerializeField] Transform tStart;
	[SerializeField] Transform tChild;
	[SerializeField] LayerMask layerMask;
	Rigidbody rb;

	void Awake(){
		rb = GetComponent<Rigidbody>();
	}
	void OnCollisionEnter(Collision collision){
		if(!tChild.gameObject.activeInHierarchy){
			return;}
		Vector3 vLink = tChild.position-tStart.position;
		RaycastHit hit;
		if(Physics.Linecast(
			tStart.position,
			tChild.position,
			out hit,
			layerMask,
			QueryTriggerInteraction.Ignore
		)){
			//if(hit.distance < vLink.magnitude){ not needed because we use Linecast
			rb.position += 
				//Vector3.Project(vLink.normalized*hit.distance*1.05f,hit.normal);
				Vector3.Project(hit.point-tChild.position,hit.normal) * 1.01f;
		}
	}
}
