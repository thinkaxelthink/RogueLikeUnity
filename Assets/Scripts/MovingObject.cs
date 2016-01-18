using UnityEngine;
using System.Collections;

public abstract class MovingObject : MonoBehaviour {

	public float moveTime = 0.1f;
	// we set this from the editor in the inspector window for this player script
	// a 2d map of all colliders on the layer labeled "BlockingLayer"
	public LayerMask blockingLayer;

	private BoxCollider2D boxCollider;

	// Rigid body seems to be a connection to 
	// a prefab GameObject (in this case on the "blockingLayer")
	private Rigidbody2D rb2D;
	private float inverseMoveTime;

	// Use this for initialization
	protected virtual void Start () {
		// When we made a prefab we added these 2 components
		boxCollider = GetComponent<BoxCollider2D> ();
		rb2D = GetComponent<Rigidbody2D> ();

		inverseMoveTime = 1f / moveTime;
	}

	// It's our first stop in moving as a Game object.
	// In here we calculate our new position & if we've collided with anything
	protected bool Move(int xDir, int yDir, out RaycastHit2D hit)
	{
		// where we were
		Vector2 start = transform.position;
		// where we are now
		Vector2 end = start + new Vector2 (xDir, yDir);

		// turn this off for now -- perhaps get one state at a time?
		boxCollider.enabled = false;
		// returns a raycasthit2d 
		// this seems to use the blocking layer as a mask
		// to figure out if the object's trajectory has encountered 
		// a collider
		hit = Physics2D.Linecast (start, end, blockingLayer);
		boxCollider.enabled = true;

		// TODO: DRY this shit up
		if (hit.transform == null) {
			StartCoroutine (SmoothMovement (end));
			return true;
		}

		return false;
	}

	// Actually moves the Object with Rigidbody2D. 
	protected IEnumerator SmoothMovement (Vector3 end)
	{
		float sqrRemainingDistance = (transform.position - end).sqrMagnitude;

		while(sqrRemainingDistance > float.Epsilon)
		{
			Vector3 newPosition = Vector3.MoveTowards(rb2D.position, end, inverseMoveTime * Time.deltaTime);
			rb2D.MovePosition (newPosition);
			sqrRemainingDistance = (transform.position - end).sqrMagnitude;
			yield return null;
		}
	}

	protected virtual void AttemptMove <T> (int xDir, int yDir)
		where T : Component
	{
		RaycastHit2D hit;
		bool canMove = Move (xDir, yDir, out hit);

		// TODO: DRY this shit up
		if (hit.transform == null) {
			return;
		}

		T hitComponent = hit.transform.GetComponent<T> ();

		if (!canMove && hitComponent != null)
		{
			OnCantMove (hitComponent);
		}
	}

	protected abstract void OnCantMove <T> (T component)
		where T : Component;
}
