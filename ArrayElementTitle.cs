using UnityEngine;
using System.Collections;
using System.Text.RegularExpressions;
 
using System;
using UnityEditor;

public class NamedArrayAttribute : PropertyAttribute {
    public Type TargetEnum;
    public string mainProp;
    public bool addId;
    public string prefix;
    public string postfix;
    public NamedArrayAttribute(string mainProp = "name", bool addId = false, Type TargetEnum = null, string prefix = "", string postfix = "") {
        this.TargetEnum = TargetEnum;
        this.mainProp = mainProp;
        this.addId = addId;
        this.prefix = prefix;
        this.postfix = postfix;
    }
}
 
#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(NamedArrayAttribute))]
public class NamedArrayDrawer : PropertyDrawer {

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
        return EditorGUI.GetPropertyHeight(property, label, property.isExpanded);
    }
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
    
        try {
            string nameGeneration = "";
            var config = attribute as NamedArrayAttribute;

            nameGeneration += config.prefix;

            if(config.TargetEnum != null){
                var enum_names = System.Enum.GetNames(config.TargetEnum);
                int pos = int.Parse(property.propertyPath.Split('[', ']')[1]);
                var enum_label = enum_names.GetValue(pos) as string;
                enum_label = ObjectNames.NicifyVariableName(enum_label.ToLower());

                nameGeneration+="     "+enum_label;
            }            
            
            SerializedProperty property2 = property.serializedObject.FindProperty(property.propertyPath+"."+config.mainProp);
            string name2 = "";

            

            if(config.mainProp == null){
            }
            else if(property2.propertyType == SerializedPropertyType.String){
                name2 = property2.stringValue;
            }
            else if(property2.propertyType == SerializedPropertyType.Integer){
                name2 = property2.intValue.ToString();
            }
            else if(property2.propertyType == SerializedPropertyType.Float){
                name2 = property2.floatValue.ToString();
            }
            else if(property2.propertyType == SerializedPropertyType.Enum){
                name2 = property2.enumNames[property2.enumValueIndex];
            }

            nameGeneration += " "+name2;

            if(config.addId){
                string getId = property.propertyPath;
                getId = getId.Substring(0, getId.LastIndexOf("]"));
                getId = getId.Substring(getId.LastIndexOf("[")+1, getId.Length-1-getId.LastIndexOf("["));
                nameGeneration += " "+getId;
            }
            
            nameGeneration += " "+config.postfix;
            
            nameGeneration = Regex.Replace(nameGeneration, @"(\S)(\s+)(\S)", "$1 $3"); 
            nameGeneration = nameGeneration.Trim();

            label = new GUIContent(nameGeneration);
        }
        catch{

        }
        EditorGUI.PropertyField(position, property, label, property.isExpanded);
    }
}
#endif