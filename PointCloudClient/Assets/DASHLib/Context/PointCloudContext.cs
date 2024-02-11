using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace KOM.DASHLib
{
    
    public class PointCloudContext : AContext
    {
        /// <summary>
        /// An point cloud prefab to be instantiated for rendering.
        /// </summary>
        [Tooltip("A prefab that will be the base for point clouds")]
        public GameObject PointCloudPrefab;
        public override ContextType Type { get; } = ContextType.Pointcloud;

        public override List<string> SupportedMimeTypes { get; } = new List<string>(){ "pointcloud/ply", "pointcloud/ply+zip", "pointcloud/drc" };

        private List<GameObject> pointClouds = new List<GameObject>();

        private Dictionary<string, PointCloudRepresentation> registeredMedia = new Dictionary<string, PointCloudRepresentation>();

        void Start()
        {
        }

        void Update()
        {
        }

        public override void Render(IRepresentation media)
        {
            PointCloudRepresentation representation = null;
            if (registeredMedia.ContainsKey(media.ID))
            {
                representation = registeredMedia[media.ID];
            }
            else
            {
                representation = new PointCloudRepresentation(media);
            }

            if (representation.State >= ERepresentationState.Prepared)
            {
                representation.State = ERepresentationState.Playing;
                GameObject pointCloud = Instantiate(PointCloudPrefab, new Vector3(0, 0, 0), Quaternion.identity);
                pointCloud.transform.localScale = transform.localScale;
                pointCloud.name = media.ID;
                pointCloud.GetComponent<MeshFilter>().sharedMesh = representation.Meshes[0]; // TODO: Play all Meshes in equal intervals

                this.pointClouds.Add(pointCloud);
                representation.State = ERepresentationState.Played;
            }
        }

        public override void Reset()
        {
            while(this.pointClouds.Count > 0)
            {                
                GameObject pointCloud = this.pointClouds.ElementAt(0);
                this.pointClouds.RemoveAt(0);
                Destroy(pointCloud);
            }
        }

        public override IRepresentation Register(IRepresentation representation)
        {
            PointCloudRepresentation pointCloudRepresentation = new PointCloudRepresentation(representation);
            if(registeredMedia.ContainsKey(pointCloudRepresentation.ID))
            {
                registeredMedia.Remove(pointCloudRepresentation.ID);
            }
            registeredMedia.Add(pointCloudRepresentation.ID, pointCloudRepresentation);
            return pointCloudRepresentation;
        }
    }
}
