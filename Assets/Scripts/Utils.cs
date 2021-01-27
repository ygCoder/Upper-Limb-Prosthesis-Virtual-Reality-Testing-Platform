using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml.Serialization;
using System.IO;

public class Utils 
{
   public static float  WrapAngle(float ang_in)
    {
        float angle = ang_in;
        angle %= 360;
        angle = angle > 180 ? angle - 360 : angle;
        return (angle);
    }

    public static Vector3 WrapAngle(Vector3 angs_in)
    {
        return (new Vector3(WrapAngle(angs_in.x), WrapAngle(angs_in.y), WrapAngle(angs_in.z)));
    }


    public static void SaveToXML<T>(string filename,T xmlvalue)
    {
        var serializer = new XmlSerializer(typeof(T));
        var stream = new FileStream("Options/"+filename+".txt", FileMode.Create);        
        XmlSerializerNamespaces a=new XmlSerializerNamespaces();
        serializer.Serialize(stream, xmlvalue);
        stream.Close();
    }
    public static T LoadFromXML<T>(string filename)
    {   
        var serializer = new XmlSerializer(typeof(T));
        var stream = new FileStream("Options/" + filename + ".txt", FileMode.Open);
        T xmlvalue = (T) serializer.Deserialize(stream);
        stream.Close();
        return (xmlvalue);
    }

    public static Matrix4x4 TransformToMatrix(Transform t)
    {
        return (Matrix4x4.TRS(t.position, t.rotation, Vector3.one));
    }
    public static void MatrixToTransform(Matrix4x4 m,Transform t)
    {
        t.position = m.GetPosition();
        t.rotation = m.GetRotation();
    }

    public static Vector3 GetAveragePosition(List<Matrix4x4> M_in)
    {
        Vector3 v_out = Vector3.zero;
        for (int i = 0; i < M_in.Count; i++)
        {
            v_out = v_out + M_in[i].GetPosition();
        }
        v_out = v_out / M_in.Count;
        return (v_out);
    }

}
