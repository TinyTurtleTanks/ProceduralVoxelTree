﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public class SpaceColonization : MonoBehaviour {
    private Tree tree;
    [HideInInspector]
    public GameObject treeObject;
    [HideInInspector]
    public GameObject voxelTree;
    [HideInInspector]
    public GameObject treeLeaves;

    public SpaceColonizationScriptableObject SCData;

    public bool showLeafRef = false;

    private bool startGen = false;
    private bool isGenerating = false;
    private bool isShown = false;
    private float curTimeoutTime = 0;

    private Material meshMaterial;

    [HideInInspector]
    public MeshFilter meshFilter;

    private Helper helper;

    public ComputeShader voxelizer;

    public string saveName = "Tree";

    private void Start() {
        helper = new Helper();
        meshMaterial = new Material(Shader.Find("Standard"));

        meshFilter = GetComponent<MeshFilter>();
    }

    void Update() {
        if (startGen) {
            SCData.showLeafRef = showLeafRef;
            if (isGenerating) {
                curTimeoutTime += Time.deltaTime;

                if (curTimeoutTime < SCData.maxTimeoutTime) {
                    tree.grow();
                } else {
                    isGenerating = false;
                }
            }

            if (!isGenerating && !isShown) {
                tree.cleanLeaves();
                tree.show();
                isShown = true;
                startGen = false;
                Debug.Log("Finished Tree Generation");
            }
        }
    }

    public void generate() {
        cleanUp();
        cleanUpVoxel();

        //new leaves object
        treeLeaves = new GameObject();
        treeLeaves.name = "Tree Leaves";

        //new tree object
        treeObject = new GameObject();
        treeObject.name = "Tree";
        treeObject.transform.position = SCData.rootPos;

        //set up mesh
        treeObject.AddComponent<MeshFilter>();
        MeshRenderer renderer = treeObject.AddComponent<MeshRenderer>();
        meshMaterial.color = SCData.branchColor;
        renderer.material = meshMaterial;

        //set up tree
        tree = treeObject.AddComponent<Tree>();
        tree.SCData = SCData;
        tree.parent = this;

        tree.setup();

        startGen = true;
        isGenerating = true;
    }

    public void clear() {
        cleanUp();
        cleanUpVoxel();
    }

    public void save() {
        //create save directory
        Directory.CreateDirectory("Assets/Meshes/" + saveName + "/Leaves");
        Directory.CreateDirectory("Assets/Meshes/" + saveName + "/Materials/Leaves");
        AssetDatabase.Refresh();

        saveMeshToDir();
        saveMatToDir();
    }

    private void saveMeshToDir() {
        MeshFilter treeFilter = treeObject.GetComponent<MeshFilter>();
        if (treeFilter) {
            // Assets/Meshes/Tree/Tree.asset
            saveAsset(saveName + "/" + saveName + ".asset", treeFilter.mesh);
        }

        if (voxelTree) {
            MeshFilter voxelFilter = voxelTree.GetComponent<MeshFilter>();

            if (voxelFilter) {
                // Assets/Meshes/Tree/TreeVoxel.asset
                saveAsset(saveName + "/" + saveName + "Voxel" + ".asset", voxelFilter.mesh);
            }
        }

        int count = 0;
        foreach (Transform child in treeLeaves.transform) {
            MeshFilter leaves = child.gameObject.GetComponent<MeshFilter>();

            if (leaves) {
                // Assets/Meshes/Tree/leaves0.asset
                saveAsset(saveName + "/Leaves/leaves" + count + ".asset", leaves.mesh);
                count++;
            }
        }
    }

    private void saveMatToDir() {
        MeshRenderer treeRenderer = treeObject.GetComponent<MeshRenderer>();
        if (treeRenderer) {
            // Assets/Meshes/Tree/Materials/Tree.mat
            saveAsset(saveName + "/Materials/" + saveName + ".mat", treeRenderer.material);
        }

        if (voxelTree) {
            MeshRenderer voxelRenderer = voxelTree.GetComponent<MeshRenderer>();

            if (voxelRenderer) {
                // Assets/Meshes/Tree/Materials/TreeVoxel.mat
                saveAsset(saveName + "/Materials/" + saveName + "Voxel" + ".mat", voxelRenderer.material);
            }
        }

        int count = 0;
        foreach (Transform child in treeLeaves.transform) {
            MeshRenderer leavesRenderer = child.gameObject.GetComponent<MeshRenderer>();

            if (leavesRenderer) {
                // Assets/Meshes/Tree/Materials/Leaves/leaves0.mat
                saveAsset(saveName + "/Materials/Leaves/leaves" + count + ".mat", leavesRenderer.material);
                count++;
            }
        }
    }

    private void saveAsset(string path, Object asset) {
        var savePath = "Assets/Meshes/" + path;
        AssetDatabase.CreateAsset(asset, savePath);
        Debug.Log("Saved Mesh to: " + savePath);
    }

    private void cleanUp() {
        //reset timeout
        curTimeoutTime = 0;
        startGen = false;
        isGenerating = false;
        isShown = false;

        //clean up old stuff
        helper.clearAllChildren(this.transform);
        Destroy(treeObject);

        Destroy(treeLeaves);
    }

    private void cleanUpVoxel() {
        Destroy(voxelTree);
    }

    public void toggleLeaves() {
        foreach (Transform child in treeLeaves.transform) {
            child.gameObject.SetActive(!child.gameObject.activeSelf);
        }
    }

    public void toggleActiveTree() {
        if (treeObject) {
            treeObject.SetActive(!treeObject.activeSelf);
        }

        if (voxelTree) {
            voxelTree.SetActive(!voxelTree.activeSelf);
        }
    }

    public void toggleShowLeafReferences() {
        foreach (Transform child in this.transform) {
            child.gameObject.SetActive(!child.gameObject.activeSelf);
        }
    }

    public MeshFilter setupVoxelize() {
        cleanUpVoxel();

        treeObject.SetActive(false);

        voxelTree = new GameObject();
        voxelTree.name = "Voxel Tree";
        voxelTree.transform.position = SCData.rootPos;
        MeshRenderer renderer = voxelTree.AddComponent<MeshRenderer>();
        renderer.material = new Material(Shader.Find("Standard"));
        renderer.material.color = SCData.branchColor;
        MeshFilter filter = voxelTree.AddComponent<MeshFilter>();
        filter.mesh = new Mesh();

        return filter;
    }
}
