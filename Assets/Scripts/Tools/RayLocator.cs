using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System;
using System.Linq;
using VOKE.VokeApp.DataModel;
using System.Xml.Serialization;
using System.IO;
using System.Xml;

public class RayLocator : MonoBehaviour
{
    public GameObject Thing;
    public GameObject Crosshair;
    public InputField IdInput;

    Dictionary<GameObject, CameraStorage> _Things = new Dictionary<GameObject, CameraStorage>();

    private class CameraStorage
    {
        public string ID { get; set; }
        public Vector2 Coordinate { get; set; }
    }

    private void Update()
    {
        var cam = Camera.main;
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);

        RaycastHit hit;
        if (!Physics.Raycast(ray, out hit))
            return;

        var pos = Vector3.MoveTowards(cam.transform.position, hit.point, 3.0f);
        var rot = Quaternion.LookRotation(hit.normal);

        Crosshair.transform.position = pos;
        Crosshair.transform.rotation = rot;

        const int leftButton = 0, rightButton = 1;
        if (Input.GetMouseButtonDown(leftButton))
        {
            string id = IdInput.text;

            if (String.IsNullOrEmpty(id))
            {
                id = (_Things.Count + 1).ToString();
            }
            else
            {
                var reader = new StringReader(id);
                id = reader.ReadLine().Trim();

                IdInput.text = reader.ReadToEnd();
            }

            var obj = (GameObject)Instantiate(Thing, pos, rot);

            var storage = new CameraStorage
            {
                ID = id,
                Coordinate = hit.textureCoord
            };

            _Things.Add(obj, storage);
        }
        else if (Input.GetMouseButtonDown(rightButton))
        {
            var go = hit.collider.gameObject;

            if (_Things.ContainsKey(go))
            {
                Destroy(go);
                _Things.Remove(go);
            }
        }
    }

    public void DestroyAll()
    {
        foreach (var t in _Things.Keys)
        {
            Destroy(t);
        }

        _Things.Clear();
    }

    public void SaveToClipboard()
    {
        var cam = new CameraStream();

        foreach (var storage in _Things.Values)
        {
            var link = new CameraLink
            {
                ID = storage.ID,
                Coordinates = storage.Coordinate.ToString("G").Trim('(', ')')
            };

            cam.Links.Add(link);
        }

        var serializer = new XmlSerializer(typeof(CameraStream));

        var emptyNamepsaces = new XmlSerializerNamespaces(new[] { XmlQualifiedName.Empty });
        var settings = new XmlWriterSettings();
        settings.Indent = true;
        settings.OmitXmlDeclaration = true;

        var sw = new StringWriter();

        using (var writer = XmlWriter.Create(sw, settings))
        {
            serializer.Serialize(writer, cam, emptyNamepsaces);
        }

        TextEditor te = new TextEditor();
        te.text = sw.ToString();
        te.SelectAll();
        te.Copy();
    }

}
