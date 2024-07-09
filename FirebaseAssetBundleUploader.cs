using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Codice.Client.Common;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using Firebase;
using Firebase.Extensions;
using Firebase.Storage;
using Firebase.Firestore;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using Cysharp.Threading.Tasks;
using System;
using Firebase.Auth;
using static FirebaseStoreModel;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
//using orcamolo;
using static CharnameDiirToEnum;

public enum BuildTargetType
{
    WebGL,
    Android,
    IOS,
    Unknown,
}

public enum AssetBundleModelReadOption
{
    charId,
    groupId,
}

public enum CharNmaes
{
    edward_black,
    eiden_marin,
    flan_sheep,
    g_luka,
    ichigo_pink,
    kevin_black,
    kevin_blue,
    kevin_purple,
    kevin_red,
    lilianne_purple,
    marianne_pink,
    mori_blue,
    mori_green,
    mori_pink,
    mori_yellow,
    neko_blue,
    neko_green,
    neko_pink,
    neko_yellow,
    noel,
    raco_chicken,
    raco_penguin,
    raco_pigeon,
    raco_shark,
    sancho_cowboy,
    sancho_hiphop,
    Spoopy_angel,
    Spoopy_devil,
    Spoopy_simple,
    Spoopy_wizard,
    suzu_cat,
    taku,
    tano_croco,
    tano_dino,
    tano_seal,
    tano_unicorn,
    umi_marin,
    yappie_dancer,
    yappie_nurse,
    yappie_police,
    yappie_student
}

public class FirebaseAssetBundleUploader : OdinMenuEditorWindow
{
    [MenuItem("AssetsBundle/Build and upload AssetBundles")]
    private static void OpenWindow()
    {
        GetWindow<FirebaseAssetBundleUploader>().Show();
    }

    protected override OdinMenuTree BuildMenuTree()
    {
        var tree = new OdinMenuTree();
        tree.Selection.SupportsMultiSelect = false;

        tree.Add("Model", new ModelUtilityEditor());
        tree.Add("Texture", new TextureUtilityEditor());

        return tree;
    }
}
public class ModelUtilityEditor
{
    FirebaseStorage storage = FirebaseStorage.DefaultInstance;
    FirebaseFirestore db = FirebaseFirestore.DefaultInstance;
    FirebaseAuth auth = FirebaseAuth.DefaultInstance;

    public BuildTargetType buildTargetType = BuildTargetType.Unknown;
    private osType _osType = osType.unknown;
    public AssetBundleModelReadOption _assetBundleModelReadOption;
    public CharNmaes _seletedChar;
    [Tooltip("INPUT FORMAT : NUM.NUM.NUM (EX :1.0.1)")]
    public string _modelVersion;
    public string _docName;
    public string _firebaseSoragePath = "gs://esper-molo.appspot.com/assetbundle/";
    StorageReference modelref;
    StorageReference gsReference;

    public FbFileModel bundleFileModel;
    public FbFileModel manifestFileModel;

    string _email = "dev.esper@esper.com";
    string _pw = "#8@,s8d<0-~fhj]<->302@";

    #region Button


    [Button(ButtonSizes.Large)]
    public void Auth()
    {
        auth.SignInWithEmailAndPasswordAsync(_email, _pw).ContinueWith(task => {
            if (task.IsCanceled)
            {
                Debug.LogError("SignInWithEmailAndPasswordAsync was canceled.");
                return;
            }
            if (task.IsFaulted)
            {
                Debug.LogError("SignInWithEmailAndPasswordAsync encountered an error: " + task.Exception);
                return;
            }

            AuthResult result = task.Result;
            Debug.LogFormat("User signed in successfully: {0} ({1})",
                result.User.DisplayName, result.User.UserId);
        });
    }

    [Button(ButtonSizes.Large)]
    public void MakeAssetBundles()
    {
        if (ValidataFormat(_modelVersion) == false || buildTargetType == BuildTargetType.Unknown || string.IsNullOrEmpty(_modelVersion))
        {
            Debug.LogError("Please check modelVersion and buildTargetType");
            return;
        }
        else
        {
            BuildTarget unityBuildTarget = ConvertToUnityBuildTarget(buildTargetType);
            string inputFolderPath = $"Assets/2Dasset/";
            string outputPath = $"Assets/AssetBundles/{_modelVersion}/{buildTargetType}/";

            if (!Directory.Exists(outputPath))
            {
                Directory.CreateDirectory(outputPath);
            }

            string[] characterFolders = Directory.GetDirectories(inputFolderPath);

            List<AssetBundleBuild> builds = new List<AssetBundleBuild>();

            foreach (string characterFolder in characterFolders)
            {
                string bundleName = Path.GetFileName(characterFolder);
                string[] assetPaths = Directory.GetFiles(characterFolder, "*", SearchOption.AllDirectories);

                AssetBundleBuild build = new AssetBundleBuild();
                build.assetBundleName = bundleName + ".ab";
                build.assetNames = assetPaths;

                builds.Add(build);
            }

            BuildPipeline.BuildAssetBundles(outputPath, builds.ToArray(), BuildAssetBundleOptions.None, unityBuildTarget);
        }
    }

    [Button(ButtonSizes.Large)]
    public void AddAssetBundle()
    {
        if (ValidataFormat(_modelVersion) == false || buildTargetType == BuildTargetType.Unknown || string.IsNullOrEmpty(_modelVersion))
        {
            Debug.LogError("Please check modelVersion and buildTargetType");
            return;
        }
        else
        {
            BuildTarget unityBuildTarget = ConvertToUnityBuildTarget(buildTargetType);
            string inputFolderPath = $"Assets/2Dasset/{_seletedChar}";
            string outputPath = $"Assets/AssetBundles/{_modelVersion}/{buildTargetType}/";
            List<AssetBundleBuild> builds = new List<AssetBundleBuild>();


            if (!Directory.Exists(outputPath))
            {
                Directory.CreateDirectory(outputPath);
            }

            string bundleName = Path.GetFileName(inputFolderPath);
            $"bundleName : {bundleName}".DLog();
            string[] assetPaths = Directory.GetFiles(inputFolderPath, "*", SearchOption.AllDirectories);

            foreach (var item in assetPaths)
            {
                $"item : {item}".DLog();
            }

            AssetBundleBuild build = new AssetBundleBuild();
            build.assetBundleName = bundleName + ".ab";
            build.assetNames = assetPaths;

            AssetBundleManifest manifest = BuildPipeline.BuildAssetBundles(outputPath, new AssetBundleBuild[] { build }, BuildAssetBundleOptions.None, unityBuildTarget);

            $"manifest.name : {manifest.name}".DLog();
        }
    }

    [Button(ButtonSizes.Large)]
    public void UpLoad()
    {
        SetBuildType();
        if (buildTargetType == BuildTargetType.Unknown || ValidataFormat(_modelVersion) == false || string.IsNullOrEmpty(_modelVersion))
        {
            Debug.LogError("Please check BuildTargetType and Model Version");
            return;
        }
        string firebasePath = $"{_firebaseSoragePath}/{_modelVersion}/{buildTargetType}";
        string bundleName = _seletedChar.ToString();
        string inputFolderPath = $"Assets/AssetBundles/{_modelVersion}";
        string[] platformFolders = Directory.GetDirectories(inputFolderPath);
        $"platformFolders : {platformFolders.Length}".DLog();
        foreach (string platformFolder in platformFolders)
        {
            if (!platformFolder.Contains(buildTargetType.ToString()))
                continue;
            $"platformFolder : {platformFolder}".DLog();
            string[] assetBundles = Directory.GetFiles(platformFolder);
            $"assetbundlesCount : {assetBundles.Length}".DLog();
            foreach (string bundlePath in assetBundles)
            {
                string fileName = Path.GetFileName(bundlePath);

                if (!FillterBuildTargetTypeString(fileName))
                {
                    $"buildTargetType.ToString() : {buildTargetType}".DLog();
                    $"bundlePath : {bundlePath}".DLog();
                    string bundleNameAll = Path.GetFileName(bundlePath);
                    $"bundleNameAll : {bundleNameAll}".DLog();
                    UploadAssetBundle(bundlePath, firebasePath, bundleNameAll);
                }
            }
        }
    }


    [Button(ButtonSizes.Large)]
    public void CreateFirestoreDoc()
    {
        SetBuildType();
        CreateVenueCharAssetBundleModel(_seletedChar.ToString());
    }

    [Button(ButtonSizes.Large)]
    public void Download()
    {
        //string firebasePath = $"gs://esper-molo.appspot.com/uploadUnity/{buildTargetType}/{FileNameToDownload}";
        //string localPath = $"Assets/AssetBundles/{buildTargetType}/{FileNameToDownload}";
        string firebasePath = $"gs://esper-molo.appspot.com/uploadUnity/{buildTargetType}";
        string localPath = $"Assets/AssetBundles/{buildTargetType}";
        gsReference = storage.GetReferenceFromUrl(firebasePath);


        /*gsReference.GetDownloadUrlAsync().ContinueWithOnMainThread(task => {
            Debug.Log("Download URL: " + task.Result);
            DownloadAssetBundle(firebasePath, localPath);
        });*/

        gsReference.GetFileAsync(localPath).ContinueWithOnMainThread(task =>
        {
            if (!task.IsFaulted && !task.IsCanceled)
            {
                Debug.Log("File downloaded.");
            }
        });
    }

    [Button(ButtonSizes.Large)]
    public void LoadAssetBundle()
    {
        if (buildTargetType == BuildTargetType.Unknown || ValidataFormat(_modelVersion) == false || string.IsNullOrEmpty(_modelVersion))
        {
            Debug.LogError("Please check BuildTargetType and Model Version");
            return;
        }
        string assetBundlesFolder = $"Assets/AssetBundles/{_modelVersion}/{buildTargetType}";
        string[] bundlePaths = Directory.GetFiles(assetBundlesFolder, "*.ab");
        foreach (string bundlePath in bundlePaths)
        {
            string bundleName = Path.GetFileName(bundlePath);

            if (bundleName.Equals($"{_seletedChar}.ab"))
            {
                LoadAssetBundle(assetBundlesFolder, bundleName);
            }
        }
    }

    [Button(ButtonSizes.Large)]
    public void LoadAssetBundleStream()
    {
        string localPath = Application.streamingAssetsPath;
        Debug.Log(localPath);
    }

    [Button(ButtonSizes.Large)]
    public async void ReadQuery()
    {
        CollectionReference collectionReference = db.Collection("venueChar");

        try
        {
            Query query = collectionReference.WhereEqualTo("charId", _seletedChar.ToString());
            QuerySnapshot querySnapshot = await query.GetSnapshotAsync();

            foreach (DocumentSnapshot doc in querySnapshot.Documents)
            {
                $"exist: {doc.Exists}, id: {doc.Id}".DLog();
                if (!doc.Exists)
                {
                    continue;
                }

                Dictionary<string, object> data = doc.ToDictionary();
                foreach (var kv in data)
                {
                    $"key : {kv.Key} , {kv.Value}".DLog();
                }

                string url = null;
                if (doc.TryGetValue("url", out object urlObject) && urlObject is string)
                {
                    url = (string)urlObject;
                    Debug.Log($"url: {url}");
                }
                else
                {
                    Debug.LogWarning("No valid URL field found");
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error reading query: {e.Message}");
        }
    }
    [Button(ButtonSizes.Large)]
    public void GetCharDir()
    {
        string[] dirs = Directory.GetDirectories("Assets/2Dasset", "*", SearchOption.AllDirectories);
        string debugString = "";

        foreach (var item in dirs)
        {
            // 디렉토리 경로에서 마지막 이름 추출
            string directoryName = new DirectoryInfo(item).Name;
            debugString += $"{directoryName},\n";
        }

        // 마지막 쉼표 제거
        if (!string.IsNullOrEmpty(debugString))
        {
            debugString = debugString.Substring(0, debugString.Length - 2);
        }

        Debug.Log(debugString);
    }
    [Button(ButtonSizes.Large)]
    public void DebugButton()
    {
        //GetCharidInDoc();
        GetFileNames();
    }

    public async Task<string> GetFileNames()
    {
        StorageReference storageRef = storage.GetReferenceFromUrl($"{_firebaseSoragePath}/{_modelVersion}");
        StorageReference folderReference = storageRef.Child(buildTargetType.ToString()); //webGL floder
        StorageReference fileReference = folderReference.Child(_seletedChar.ToString() + ".ab"); //file fullname


        try
        {
            // 폴더의 메타데이터 가져오기
            StorageMetadata metadata = await fileReference.GetMetadataAsync();

            if (metadata != null)
            {
                // 메타데이터에서 파일 이름 출력
                Debug.Log("File Name: " + metadata.Name);
                return metadata.Name;
            }
            else
            {
                Debug.LogError("Folder metadata is null");
                return null;
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Error getting metadata: " + e.Message);
            return null;
        }
    }



    #endregion
    void UploadAssetBundle(string bundlePath, string firebasePath, string bundleName)
    {
        string firebaseBundlePath = $"{firebasePath}"; //check $"gs://esper-molo.appspot.com/assetbundle/{_modelVersion}/{buildTargetType}"
        StorageReference storageRef = storage.GetReferenceFromUrl(firebaseBundlePath);
        StorageReference targetFileRef = storageRef.Child(bundleName);

        targetFileRef.PutFileAsync(bundlePath).ContinueWith(async (Task<StorageMetadata> task) =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                Debug.Log(task.Exception.ToString());
            }
            else
            {
                if (task.IsCompleted)
                {
                    $"Upload Finish".DLog();
                    StorageMetadata metadata = task.Result;
                    $"task result : {metadata}".DLog();
                }
            }
        });
    }


    private async void DownloadAssetBundle(string url, string localPath)
    {
        UnityWebRequest www = UnityWebRequest.Get(url);
        {
            await www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                // Download successful
                System.IO.File.WriteAllBytes(localPath, www.downloadHandler.data);
            }
            else
            {
                Debug.LogError("Failed to download asset bundle: " + www.error);
            }
        }
    }

    void LoadAssetBundle(string assetBundlesFolder, string bundleName)
    {
        string bundlePath = Path.Combine(assetBundlesFolder, bundleName);


        AssetBundle bundle = AssetBundle.LoadFromFile(bundlePath);
        if (bundle == null)
        {
            Debug.LogError("Failed to load AssetBundle: " + bundleName);
            return;
        }


        string[] assetNames = bundle.GetAllAssetNames();
        foreach (string assetName in assetNames)
        {

            if (assetName.EndsWith(".prefab", StringComparison.OrdinalIgnoreCase))
            {
                GameObject loadedAsset = bundle.LoadAsset<GameObject>(assetName);
                if (loadedAsset != null)
                {
                    Debug.Log("Loaded prefab from " + bundleName + ": " + assetName);
                    UnityEngine.Object.Instantiate(loadedAsset);
                }
                else
                {
                    Debug.LogError("Failed to load prefab from " + bundleName + ": " + assetName);
                }
            }
        }


        bundle.Unload(false);
    }

    BuildTarget ConvertToUnityBuildTarget(BuildTargetType targetType)
    {
        switch (targetType)
        {
            case BuildTargetType.WebGL:
                _osType = osType.web;
                return BuildTarget.WebGL;
            case BuildTargetType.Android:
                _osType = osType.android;
                Debug.Log($"_osType : {_osType}");
                return BuildTarget.Android;
            case BuildTargetType.IOS:
                _osType = osType.ios;
                Debug.Log($"_osType : {_osType}");
                return BuildTarget.iOS;
            default:
                _osType = osType.unknown;
                Debug.Log($"_osType : {_osType}");
                return BuildTarget.NoTarget;
        }
    }

    private void SetBuildType()
    {
        switch (buildTargetType)
        {
            case BuildTargetType.WebGL:
                _osType = osType.web;
                $" osType : {_osType}, buildTargetType :{buildTargetType}".DLog();
                break;
            case BuildTargetType.Android:
                _osType = osType.android;
                $" osType : {_osType}, buildTargetType :{buildTargetType}".DLog();
                break;
            case BuildTargetType.IOS:
                _osType = osType.ios;
                $" osType : {_osType}, buildTargetType :{buildTargetType}".DLog();
                break;
            default:
                _osType = osType.unknown;
                $" osType : {_osType}, buildTargetType :{buildTargetType}".DLog();
                break;
        }
    }

    public bool ValidataFormat(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            Debug.LogError("Please check BuildTargetType and Model Version");
        }
        string pattern = @"^\d+(\.\d+){2}$";

        return Regex.IsMatch(input, pattern);
    }


    #region ModelMethod

    public string ExtractGroupId(string fileName)
    {
        string[] parts = fileName.Split('_');

        string groupId = parts[0];

        return groupId;
    }



    private string GenDocId(string path = "_")
    {
        return db.Collection(path).Document().Id;
    }

    private string GetUploaderUid()
    {
        FirebaseUser user = FirebaseAuth.DefaultInstance.CurrentUser;
        return user.UserId;
    }

    public async Task<FbFileModel> GetBundleFileModelAsync()
    {
        StorageReference storageRef = storage.GetReferenceFromUrl($"{_firebaseSoragePath}/{_modelVersion}/{buildTargetType}");
        StorageReference targetFileRef = storageRef.Child(_seletedChar.ToString() + ".ab");

        StorageMetadata meta = await targetFileRef.GetMetadataAsync();
        Uri downloadUrl = await targetFileRef.GetDownloadUrlAsync();

        bundleFileModel = new FbFileModel()
        {
            url = downloadUrl.AbsoluteUri,
            fullpath = targetFileRef.Path,
            createdAt = meta.CreationTimeMillis,
            updatedAt = meta.UpdatedTimeMillis,
            filename = meta.Name,
            id = GenDocId(),
            refCount = 999,
            uploaderUid = GetUploaderUid(),
            type = FbFileType.file,
        };
        Debug.Log($"bundleFileModel.url: {bundleFileModel.url}");
        Debug.Log($"bundleFileModel.fullPath: {bundleFileModel.fullpath}");
        Debug.Log($"bundleFileModel.createdAt: {bundleFileModel.createdAt}");
        Debug.Log($"bundleFileModel.updatedAt: {bundleFileModel.updatedAt}");
        Debug.Log($"bundleFileModel.filename: {bundleFileModel.filename}");
        Debug.Log($"bundleFileModel.id: {bundleFileModel.id}");
        Debug.Log($"bundleFileModel.refCount: {bundleFileModel.refCount}");
        Debug.Log($"bundleFileModel.uploaderUid: {bundleFileModel.uploaderUid}");
        Debug.Log($"bundleFileModel.type: {bundleFileModel.type}");
        return bundleFileModel;
    }

    public async Task<FbFileModel> GetManifestFileModelAsync()
    {
        StorageReference storageRef = storage.GetReferenceFromUrl($"{_firebaseSoragePath}/{_modelVersion}/{buildTargetType}");
        StorageReference targetFileRef = storageRef.Child(_seletedChar.ToString() + ".ab.manifest");

        StorageMetadata meta = await targetFileRef.GetMetadataAsync();
        Uri downloadUrl = await targetFileRef.GetDownloadUrlAsync();

        manifestFileModel = new FbFileModel()
        {
            url = downloadUrl.AbsoluteUri,
            fullpath = targetFileRef.Path,
            createdAt = meta.CreationTimeMillis,
            updatedAt = meta.UpdatedTimeMillis,
            filename = meta.Name,
            id = GenDocId(),
            refCount = 999,
            uploaderUid = GetUploaderUid(),
            type = FbFileType.file,
        };

        Debug.Log($"manifestFileModel.url: {manifestFileModel.url}");
        Debug.Log($"manifestFileModel.fullPath: {manifestFileModel.fullpath}");
        Debug.Log($"manifestFileModel.createdAt: {manifestFileModel.createdAt}");
        Debug.Log($"manifestFileModel.updatedAt: {manifestFileModel.updatedAt}");
        Debug.Log($"manifestFileModel.filename: {manifestFileModel.filename}");
        Debug.Log($"manifestFileModel.id: {manifestFileModel.id}");
        Debug.Log($"manifestFileModel.refCount: {manifestFileModel.refCount}");
        Debug.Log($"manifestFileModel.uploaderUid: {manifestFileModel.uploaderUid}");
        Debug.Log($"manifestFileModel.type: {manifestFileModel.type}");

        return manifestFileModel;
    }

    public async void CreateVenueCharAssetBundleModel(string charIdinStorage)
    {
        try
        {
            DocumentReference docRef = db.Collection("venueCharAssetBundle").Document();

            var assetBundleModel = new VenueCharAssetBundleModel()
            {
                bundleId = docRef.Id,
                charId = charIdinStorage, // get VenueCharModel doc
                createdAt = DateTime.UtcNow,
                osType = _osType,
                unityVersion = Application.unityVersion,
                modelVersion = _modelVersion,
                bundleFile = await GetBundleFileModelAsync(),
                manifestFile = await GetManifestFileModelAsync(),
            };

            var bundleFileData = PrepareFileData(assetBundleModel.bundleFile);
            var manifestFileData = PrepareFileData(assetBundleModel.manifestFile);

            var data = new Dictionary<string, object>();
            var properties = typeof(VenueCharAssetBundleModel).GetProperties();
            foreach (var property in properties)
            {
                if (property.Name == "bundleFile")
                {
                    data[property.Name] = bundleFileData;
                }
                else if (property.Name == "manifestFile")
                {
                    data[property.Name] = manifestFileData;
                }
                else
                {
                    if (property.Name == "osType")
                    {
                        data[property.Name] = assetBundleModel.osType.ToString();
                    }
                    else
                    {
                        data[property.Name] = property.GetValue(assetBundleModel);
                    }
                }
            }
            foreach (var item in data)
            {
                Debug.Log($"Key: {item.Key}, Value: {item.Value}");
            }

            await docRef.SetAsync(data);
        }
        catch (Exception e)
        {
            Debug.LogError("AssetBundle 문서 설정 오류: " + e.Message);
        }
    }

    /*public void CreateVenueCharAssetBundleModel(Task<StorageMetadata> task, string charIdinStorage)
    {
        if (!task.IsCompleted)
        {
            Debug.LogError("Task<StorageMetadata>가 아직 완료되지 않았습니다.");
            return;
        }
        DocumentReference docRef = db.Collection("venueCharAssetBundle").Document();
        var assetBundleModel = new VenueCharAssetBundleModel()
        {
            bundleId = docRef.Id,
            charId = charIdinStorage, // VenueCharModel fire store에서 읽어오자
            createdAt = DateTime.UtcNow,
            osType = _osType,
            unityVersion = Application.unityVersion,
            modelVersion = _modelVersion,
            bundleFile = GetBundleFileModelAsync().Result,
            manifestFile = GetManifestFileModelAsync().Result,
        };

        try
        {
            // file 객체의 데이터를 딕셔너리에 저장하기 전에 type 필드를 문자열로 변환
            var bundleFileData = new Dictionary<string, object>();
            var bundleFileProperties = typeof(FbFileModel).GetProperties();
            foreach (var fileProperty in bundleFileProperties)
            {
                if (fileProperty.Name == "type") // 열거형 필드 이름 확인
                {
                    // 열거형 값을 문자열로 변환하여 저장
                    bundleFileData[fileProperty.Name] = assetBundleModel.bundleFile.type.ToString();
                }
                else
                {
                    // 그 외의 필드는 기존 로직대로 처리
                    bundleFileData[fileProperty.Name] = fileProperty.GetValue(assetBundleModel.bundleFile);
                }
            }

            var manifestFileData = new Dictionary<string, object>();
            var manifestFileProperties = typeof(FbFileModel).GetProperties();
            foreach (var fileProperty in manifestFileProperties)
            {
                if (fileProperty.Name == "type") // 열거형 필드 이름 확인
                {
                    // 열거형 값을 문자열로 변환하여 저장
                    manifestFileData[fileProperty.Name] = assetBundleModel.manifestFile.type.ToString();
                }
                else
                {
                    // 그 외의 필드는 기존 로직대로 처리
                    manifestFileData[fileProperty.Name] = fileProperty.GetValue(assetBundleModel.manifestFile);
                }
            }

            var data = new Dictionary<string, object>();
            var properties = typeof(VenueCharAssetBundleModel).GetProperties();
            foreach (var property in properties)
            {
                if (property.Name == "bundleFile")
                {
                    // file 필드의 경우, 위에서 준비한 딕셔너리 사용
                    data[property.Name] = bundleFileData;
                    $"data[property.Name] bundlefile : {data[property.Name]}".DLog();
                }
                if (property.Name == "manifestFile")
                {
                    // file 필드의 경우, 위에서 준비한 딕셔너리 사용
                    data[property.Name] = manifestFileData;
                    $"data[property.Name] manifest : {data[property.Name]}".DLog();
                }
                else
                {

                    if (property.Name == "osType")
                    {
                        data[property.Name] = assetBundleModel.osType.ToString();
                    }
                    else data[property.Name] = property.GetValue(assetBundleModel);
                }
            }
            foreach (var item in data)
            {
                Debug.Log($"Key: {item.Key}, Value: {item.Value}");
            }
            docRef.SetAsync(data);
        }
        catch (Exception e)
        {
            Debug.LogError("AssetBundle 문서 설정 오류: " + e.Message);
        }
    }*/
    /*public void CreateVenueCharAssetBundleModel(Task<StorageMetadata> task, string charIdinStorage)
    {
        if (!task.IsCompleted)
        {
            Debug.LogError("Task<StorageMetadata>가 아직 완료되지 않았습니다.");
            return;
        }
        DocumentReference docRef = db.Collection("venueCharAssetBundle").Document();
        var assetBundleModel = new VenueCharAssetBundleModel()
        {
            bundleId = docRef.Id,
            charId = charIdinStorage, // VenueCharModel fire store에서 읽어오자
            createdAt = DateTime.UtcNow,
            osType = _osType,
            unityVersion = Application.unityVersion,
            modelVersion = _modelVersion,
            file = new FbFileModel()
            {
                url = task.Result.Reference.GetDownloadUrlAsync().Result.AbsoluteUri,
                fullPath = task.Result.Path,
                createdAt = task.Result.CreationTimeMillis,
                updatedAt = task.Result.UpdatedTimeMillis,
                filename = task.Result.Name,
                id = GenDocId(),
                refCount = 999,
                uploaderUid = GetUploaderUid(),
                type = FbFileType.file,
            }
        };
        
        //에러나는 부분
        try
        {
            {
                var fileData = new Dictionary<string, object>();
                var fileProperties = assetBundleModel.file.GetType().GetProperties();
                foreach (var fileProperty in fileProperties)
                {
                    if (fileProperty.Name == "type") // 열거형 필드 이름 확인
                    {
                        // 열거형 값을 문자열로 변환하여 저장
                        fileData[fileProperty.Name] = assetBundleModel.file.type.ToString();
                    }
                    else
                    {
                        // 그 외의 필드는 기존 로직대로 처리
                        fileData[fileProperty.Name] = fileProperty.GetValue(assetBundleModel.file);
                    }
                }
            }
            var data = new Dictionary<string, object>();
            var properties = assetBundleModel.GetType().GetProperties();
            foreach (var property in properties)
            {
                if (property.Name == "file")
                {
                    // file 필드의 경우, 위에서 준비한 딕셔너리 사용
                    data[property.Name] = fileData;
                }
                if (property.PropertyType.IsEnum)
                {
                    // Enum 속성의 값을 문자열로 변환하여 저장
                    var enumValue = property.GetValue(assetBundleModel);
                    var stringValue = JsonConvert.SerializeObject(enumValue, new EnumStringConverter());
                    $"convert value : {stringValue}".DLog();
                    data[property.Name] = stringValue;
                }
                else
                {
                    // Enum이 아닌 다른 속성은 그대로 저장
                    data[property.Name] = property.GetValue(assetBundleModel);
                }
            }
            docRef.SetAsync(data);
            foreach (var item in data)
            {
                Debug.Log($"Key: {item.Key}, Value: {item.Value}");
            }
            //Debug.Log("AssetBundle 문서 생성");
        }
        catch (Exception e)
        {
            Debug.LogError("AssetBundle 문서 설정 오류: " + e.Message);
        }
    }

    public static string ConvertEnumToString(object enumValue)
    {
        if (enumValue == null)
        {
            return null;
        }

        var enumType = enumValue.GetType();
        if (!enumType.IsEnum)
        {
            return enumValue.ToString();
        }

        var enumName = Enum.GetName(enumType, enumValue);
        if (string.IsNullOrEmpty(enumName))
        {
            return null;
        }

        return enumName;
    }

    /*public VenueCharModel CreateVenueCharModel(Task<StorageMetadata> task, string docName)
    {
        DocumentReference docRef = db.Collection("venueChar").Document(docName);
        var charModel = new VenueCharModel()
        {
            charId = docRef.Id,
            displayName = docName,
            visible = true,
            cost = 0,
            createdAt = task.Result.UpdatedTimeMillis,
            groupId = ExtractGroupId(docName),
        };
        docRef.SetAsync(charModel);
        Debug.Log("Create charmodel doc");
        return charModel;
    }*/



    private bool FillterBuildTargetTypeString(string fileName)
    {
        string[] enumNameArray = Enum.GetNames(buildTargetType.GetType());
        foreach (var name in enumNameArray)
        {
            if (fileName.Contains(name))
            {
                return true;
            }
        }
        return false;
    }

    private Dictionary<string, object> PrepareFileData(FbFileModel fileModel)
    {
        var fileData = new Dictionary<string, object>();
        var fileProperties = typeof(FbFileModel).GetProperties();
        foreach (var fileProperty in fileProperties)
        {
            if (fileProperty.Name == "type")
            {
                fileData[fileProperty.Name] = fileModel.type.ToString();
            }
            else
            {
                fileData[fileProperty.Name] = fileProperty.GetValue(fileModel);
            }
        }
        return fileData;
    }


    string GetBuildTargetTypeString(BuildTargetType targetType)
    {
        switch (targetType)
        {
            case BuildTargetType.WebGL:
                return "WebGL";
            case BuildTargetType.Android:
                return "Android";
            case BuildTargetType.IOS:
                return "IOS";
            case BuildTargetType.Unknown:
                return "Unknown";
            default:
                return "Unknown";
        }
    }
    #endregion


}


public class TextureUtilityEditor
{
    public string firebasePath;//= "esper-molo.appspot.com";
    public BuildTarget buildTarget;

    public List<Texture> Textures;

    [Button(ButtonSizes.Large)]
    public void UpLoad()
    {
        //BuildPipeline.BuildAssetBundles("Assets/AssetBundles/", BuildAssetBundleOptions.None, buildTarget);

        string[] assetBundles = AssetDatabase.GetAllAssetBundleNames();
        foreach (string bundleName in assetBundles)
        {
            string bundlePath = $"Assets/AssetBundles/";
            UploadAssetBundle(bundlePath, firebasePath);
        }
    }

    void UploadAssetBundle(string bundlePath, string firebasePath)
    {
        byte[] bundleData = System.IO.File.ReadAllBytes(bundlePath);

        using (UnityWebRequest www = UnityWebRequest.Put(firebasePath, bundleData))
        {
            www.SendWebRequest();

            while (!www.isDone)
            {
                Debug.Log("Running");
            }

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Error uploading AssetBundle {bundlePath}: {www.error}");
            }
            else
            {
                Debug.Log($"AssetBundle {bundlePath} uploaded successfully!");
            }
        }
    }
}




public static class Nyang
{
    public static void DLog(this string s)
    {
        Debug.Log(s);
    }
}