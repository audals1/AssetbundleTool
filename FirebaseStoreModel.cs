using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Firestore;
using Firebase.Extensions;
using Newtonsoft.Json;
using System;
using UnityEditor;
using System.Linq;
using orcamolo;



public class FirebaseStoreModel
{
    public enum osType
    {
        web,
        android,
        ios,
        unknown,
    }

    public enum FbFileType
    {
        image,
        audio,
        video,
        text,
        file,
        unknown,
    }



    [FirestoreData]
    public class FbFileModel //StorageMetadata.result 때려넣음
    {
        [FirestoreProperty]
        public string id { get; set; }
        [FirestoreProperty]
        public string uploaderUid { get; set; }

        [FirestoreProperty]
        [JsonConverter(typeof(EnumStringConverter))]
        public FbFileType type { get; set; }

        [FirestoreProperty]
        public int refCount { get; set; }

        [FirestoreProperty]
        public int width { get; set; }
        [FirestoreProperty]
        public int height { get; set; }
        [FirestoreProperty]
        public string url { get; set; }

        [FirestoreProperty]
        public string filename { get; set; }
        [FirestoreProperty]
        public string fullpath { get; set; }
        [FirestoreProperty]
        public DateTime updatedAt { get; set; }
        [FirestoreProperty]
        public DateTime createdAt { get; set; }
        [FirestoreProperty]
        public List<FbFileModel> thumbnails { get; set; }
    }


        [FirestoreData]
        public class VenueCharAssetBundleModel
        {
            [FirestoreProperty]
            public string bundleId { get; set; }
            [FirestoreProperty]
            public string charId { get; set; }
            [FirestoreProperty]
            [JsonConverter(typeof(EnumStringConverter))]
            public osType osType { get; set; }
            [FirestoreProperty]
            public string unityVersion { get; set; }
            [FirestoreProperty]
            public string modelVersion { get; set; }

            [FirestoreProperty]
            public DateTime createdAt { get; set; }

            [FirestoreProperty]
            [JsonConverter(typeof(EnumStringConverter))]
            public FbFileModel bundleFile { get; set; }

            [FirestoreProperty]
            [JsonConverter(typeof(EnumStringConverter))]
            public FbFileModel manifestFile { get; set; }
        }

        [FirestoreData]
        public class VenueCharModel
        {
            [FirestoreProperty]
            public string charId { get; set; } //document id
            [FirestoreProperty]
            public string displayName { get; set; }
            [FirestoreProperty]
            public bool visible { get; set; }
            [FirestoreProperty]
            public int cost { get; set; }

            [FirestoreProperty]
            public string groupId { get; set; }
            [FirestoreProperty]
            public DateTime createdAt { get; set; }
        }

        [FirestoreData]
        public class QuestModel
    {
        [FirestoreProperty]
        public string completeCount { get; set; }
    }
}

