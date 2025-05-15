using UnityEngine;
using Photon.Pun;

[RequireComponent(typeof(PhotonView))]
public class UVObject : MonoBehaviourPun
{
    private Renderer rend;
    private MaterialPropertyBlock propertyBlock;

    void Start()
    {
        rend = GetComponent<Renderer>();
        propertyBlock = new MaterialPropertyBlock();

        if (rend != null)
        {
            rend.material = new Material(rend.material); // Copie le material pour ce GameObject
            rend.GetPropertyBlock(propertyBlock);
            propertyBlock.SetFloat("_UVIntensity", 0f);
            rend.SetPropertyBlock(propertyBlock);
        }
    }

    [PunRPC]
    public void SetUVHit(Vector3 hitPos)
    {
        if (rend == null)
            rend = GetComponent<Renderer>();
        if (propertyBlock == null)
            propertyBlock = new MaterialPropertyBlock();

        rend.GetPropertyBlock(propertyBlock);
        propertyBlock.SetFloat("_UVIntensity", 5f); // ou une valeur adaptée pour ton shader
        propertyBlock.SetVector("_HitPosition", hitPos);
        rend.SetPropertyBlock(propertyBlock);
    }
}