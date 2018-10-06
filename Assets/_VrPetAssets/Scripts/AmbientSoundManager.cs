using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmbientSoundManager : MonoBehaviour {

    private List<AmbientSound> ambienceGenerators = new List<AmbientSound>();

	void Start () 
	{
		for (int i = 0; i < transform.childCount; i++)
        {
            AmbientSound generator = transform.GetChild(i).GetComponent<AmbientSound>();
            if (generator)
            {
                ambienceGenerators.Add(generator);
            }
        }
	}
	
    public void UpdateVolume(float newVolume)
    {
        foreach (AmbientSound generator in ambienceGenerators)
        {
            generator.volumeSliderModifier = newVolume;
        }
    }
}
