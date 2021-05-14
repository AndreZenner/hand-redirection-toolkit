using System.Collections;
using System.Collections.Generic;
using HR_Toolkit;
using UnityEngine;
using System.Linq;

public class Zenner_Regitz_Krueger_BodyWarping_BSHR : BodyWarping
{
    public float betaMax = 4.5f;
    public float gMin = 0.94f;
    public float gMax = 1.14f;

    public bool testWithKeyB = false;


    private Vector3 o,v,p,ov,op,vp;
    private Vector3 b;      // offset vector
    private Vector3 p_;     // dummy target
    private Vector3 vp_;


    public override void Init(RedirectionObject redirectionObject, Transform head, Vector3 warpOrigin)
    {

        v = redirectionObject.GetVirtualTargetPos();         // Virtual Target V
        p = redirectionObject.GetRealTargetPos();            // Real Target P
        o = warpOrigin;                                      // Origin O

        ov = v - o;                     // Vector origin O -> virtual target V
        op = p - o;                     // Vector origin O -> real target P
        vp = p-v;                       // Vector real target P -> virtual target V
        b = Vector3.zero;               // offset vector is set to 0 at the start
 
        ComputeDummyTarget();
        vp_ = p_ - v;
        if (!testWithKeyB)
        {
            GetComponent<BlinkDetector>().running = true;
            GetComponent<BlinkDetector>().blinked = false;
        }

    }

    public override void ApplyRedirection(Transform realHandPos, Transform virtualHandPos, Transform warpOrigin, RedirectionObject target,
        Transform bodyTransform)
    {
        if (testWithKeyB)
        {
            if (Input.GetKeyDown(KeyCode.B))
            {
                b = p_ - p;
            }
        }
        else if (GetComponent<BlinkDetector>().blinked){
            b = p_ - p;
        }
  
        float ds = ((realHandPos.position + b) - warpOrigin.position).magnitude;
        float dp = ((realHandPos.position + b) - p_).magnitude;

        float alpha = ds / (ds + dp);
        Vector3 w = alpha * (v - p_) + b;

        virtualHandPos.position = realHandPos.position + w; 

    }

    public void ComputeDummyTarget(){
        // Each of the computed intersections is represented by a value t ∈ R (see Equation 5) denoting the relative position of the intersection
        // along the ray (t = 0 representing V ; t = 1 representing P).
        // To determine P′ as the location that is closest to the target P but still within all thresholds,
        // the BSHR algorithm sorts all corresponding t ≥ 0 in ascending order
        List<float> T = new List<float>();

        // The physical target itself
        T.Add(1);

        // points where betaMax is exceeded (right and left plane)
        Vector3 intersectionRayPlaneRight,intersectionRayPlaneLeft;
        if(IntersectionRayPlane(out intersectionRayPlaneRight, betaMax)){
            float relativeIntersectionPoint = GetRelativePostionOfIntersection(intersectionRayPlaneRight);
            T.Add(relativeIntersectionPoint);
        } 
        if(IntersectionRayPlane(out intersectionRayPlaneLeft, -betaMax)){
            float relativeIntersectionPoint = GetRelativePostionOfIntersection(intersectionRayPlaneLeft);
            T.Add(relativeIntersectionPoint);
        }

        // points where gMax is exceededs
        List<Vector3> intersectionGMax;
        if (IntersectionRaySphere(out intersectionGMax, gMax)){
            foreach(Vector3 point in intersectionGMax){
                float relativeIntersectionPoint = GetRelativePostionOfIntersection(point);
                T.Add(relativeIntersectionPoint);
            }  
        } 

        // get current minimum value
        float minimumValue = T.Min();

        //points where gMin is exceeded
        int gMinIntersections = 0;
        List<Vector3> intersectionGMin;
        if (IntersectionRaySphere(out intersectionGMin, gMin)){
            foreach(Vector3 point in intersectionGMin){
                float relativeIntersectionPoint = GetRelativePostionOfIntersection(point);
                T.Add(relativeIntersectionPoint);
                if(relativeIntersectionPoint < minimumValue) gMinIntersections++;
            }
        }
            
        // exception: both gmin intersections are smallest values
        Vector3 positionDummyTarget;
        if(gMinIntersections==2){
            positionDummyTarget= v + (minimumValue * vp);
        } else {
            positionDummyTarget = v + (T.Min() * vp);
        }

        p_ =  positionDummyTarget;
    }

    // Calculates the intersection point of the ray vp and the rotation plane betaMax
    private bool IntersectionRayPlane(out Vector3 intersection, float rotationAngle){

        // Get perpendicular axis which will be the rotation axis
        Vector3 rotationAxis = Vector3.Cross(ov,op);

        // Rotate Vector ov around 'rotationAxis'
        Vector3 ovRotated = Quaternion.AngleAxis(rotationAngle, rotationAxis) * ov;

        // Calculate the plane normal vector 
        Vector3 planeNormal = (Vector3.Cross(ovRotated,rotationAxis)).normalized;

        // Get intersection point of ray vp and plane 
        if(Math3d.LinePlaneIntersection(out intersection, v, vp.normalized,planeNormal,o)){
            bool sameDirectionVPIntersection =  pointsSameDirectionOnLine(intersection-v, vp);
            bool sameDirectionPlaneIntersection = pointsSameDirectionOnLine(intersection-o, ovRotated);
            return sameDirectionVPIntersection && sameDirectionPlaneIntersection;
        }
        return false;
    }


    // Calculates the intersection point of the ray and a sphere between P and V
    private bool IntersectionRaySphere(out List<Vector3> intersectionList, float gain) {

        intersectionList = new List<Vector3>();
        List<Vector3> intersectionpoints;

        // Calculate radius of Sphere
        float radius = (gain*ov).magnitude;

        //Get intsection points of ray vp and a sphere
        if(LineSphereIntersection(out intersectionpoints, o, radius, v, p)){
           
            foreach(Vector3 point in intersectionpoints){
                if (pointsSameDirectionOnLine(point-v, vp)){
                    intersectionList.Add(point);                
                }
            }
            return intersectionList.Count != 0;
        }
        return false;

    }


    public float GetRelativePostionOfIntersection(Vector3 intersection){
        return (intersection - v).magnitude / vp.magnitude;
    }

    public Vector3 GetVectorVP_(){
        return vp_;
    }

     //https://answers.unity.com/questions/869869/method-of-finding-point-in-3d-space-that-is-exactl.html
     public static bool LineSphereIntersection(out List<Vector3> hitPoints, Vector3 center, float radius, Vector3 rayStart, Vector3 rayEnd)
     {

        hitPoints = new List<Vector3>();         
        Vector3 directionRay = rayEnd - rayStart;
        Vector3 centerToRayStart = rayStart - center;

        float a = Vector3.Dot(directionRay, directionRay);
        float b = 2 * Vector3.Dot(centerToRayStart, directionRay);
        float c = Vector3.Dot(centerToRayStart, centerToRayStart) - (radius * radius);

        float discriminant = (b * b) - (4 * a * c);
        if (discriminant >= 0)
        {
            //Ray did not miss
            discriminant = Mathf.Sqrt(discriminant);

            //How far on ray the intersections happen
            float t1 = (-b - discriminant) / (2 * a);
            float t2 = (-b + discriminant) / (2 * a);

            hitPoints = new List<Vector3>();

            if (t1 >= 0 && t2 >= 0 )
            {
                //total intersection, return both points
                hitPoints.Add(rayStart + (directionRay * t1));
                hitPoints.Add(rayStart + (directionRay * t2));
            }
            else
            {
                //Only one intersected, return one point
                if (t1 >= 0 )
                {
                    hitPoints.Add(rayStart + (directionRay * t1));
                }
                else if (t2 >= 0 )
                {
                    hitPoints.Add(rayStart + (directionRay * t2));
                }
            }
            return true;
        }
        //No hits
        return false;
     }

    public static bool pointsSameDirectionOnLine(Vector3 a, Vector3 b){
        float direction = Vector3.Dot(a.normalized,b.normalized);
        return direction > 0; 
    }

}
