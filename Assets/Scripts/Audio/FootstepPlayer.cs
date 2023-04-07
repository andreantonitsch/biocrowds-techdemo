using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FootstepPlayer : MonoBehaviour
{

    [SerializeField] private AudioAsset _footstepAsset;
    [SerializeField] private AudioSource _audioSource;

    [SerializeField] private Vector2 _randomMinMax;

    private float _playCount = 0f;

    private void Start()
    {
        _randomMinMax *= Random.Range(1f, 3f);

    }

    private IEnumerator PlayCoroutine()
    {

        yield return new WaitForSeconds(Random.Range(_randomMinMax.x, _randomMinMax.y));

        _audioSource.Stop();

        _audioSource.clip = _footstepAsset.Play();

        _audioSource.Play();

        //if()
        _playCount--;

    }

    public void Play()
    {


        if (_playCount > 1)
        {
            return;
        }

        if(Random.Range(0f,1f) > 0.5)
        {
            return;
        }



        _playCount++;
        StartCoroutine(PlayCoroutine());
    }
  
}
