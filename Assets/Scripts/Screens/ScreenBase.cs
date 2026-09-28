using UnityEngine;

public class ScreenBase : MonoBehaviour
{
    [SerializeField] private ScreenId _id;

    public ScreenId Id => _id;
}