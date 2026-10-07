using UnityEngine;

public class Piano : MonoBehaviour
{
    public AudioClip[] audioClips;
    public AudioSource audioSource;
    string btnName;
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                btnName = hit.collider.name;
                switch(btnName)
                {
                    case "Do":
                        audioSource.clip = audioClips[0];
                        audioSource.Play();
                        break;
                    case "Re":
                        audioSource.clip = audioClips[1];
                        audioSource.Play();
                        break;
                    case "Mi":
                        audioSource.clip = audioClips[2];
                        audioSource.Play();
                        break;
                    case "Fa":
                        audioSource.clip = audioClips[3];
                        audioSource.Play();
                        break;
                    case "Sol":
                        audioSource.clip = audioClips[4];
                        audioSource.Play();
                        break;
                    case "La":
                        audioSource.clip = audioClips[5];
                        audioSource.Play();
                        break;
                    case "Si":
                        audioSource.clip = audioClips[6];
                        audioSource.Play();
                        break;
                    default:
                        break;

                }
            }
        }
    }
}
