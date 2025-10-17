using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Map
{
    public class StairChane : MonoBehaviour
    {
        public Direction direction;                                 //direction of the stairs
        [Space]
        public string sortingLayerUpper;
        [Space]
        public string sortingLayerLower;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (direction == Direction.South && other.transform.position.y < transform.position.y) SetLayerAndSortingLayer(other.gameObject, sortingLayerUpper);
            else
            if (direction == Direction.West && other.transform.position.x < transform.position.x) SetLayerAndSortingLayer(other.gameObject, sortingLayerUpper);
            else
            if (direction == Direction.East && other.transform.position.x > transform.position.x) SetLayerAndSortingLayer(other.gameObject, sortingLayerUpper);
            print("Direction: " + direction);
            print("Sorting Layer Change to: " + sortingLayerUpper);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (direction == Direction.South && other.transform.position.y < transform.position.y) SetLayerAndSortingLayer(other.gameObject, sortingLayerLower);
            else
            if (direction == Direction.West && other.transform.position.x < transform.position.x) SetLayerAndSortingLayer(other.gameObject, sortingLayerLower);
            else
            if (direction == Direction.East && other.transform.position.x > transform.position.x) SetLayerAndSortingLayer(other.gameObject, sortingLayerLower);
            print("Direction: " + direction);
            print("Sorting Layer Change to: " + sortingLayerLower);
        }

        private void SetLayerAndSortingLayer( GameObject target, string sortingLayer )
        {
            
            print("Current layer: " + target.GetComponent<SpriteRenderer>().sortingLayerName);
            target.GetComponent<SpriteRenderer>().sortingLayerName = sortingLayer;
            SpriteRenderer[] srs = target.GetComponentsInChildren<SpriteRenderer>();
            foreach (SpriteRenderer sr in srs)
            {
                sr.sortingLayerName = sortingLayer;
            }
        }

        public enum Direction
        {
            North,
            South,
            West,
            East
        }    
    }
}
