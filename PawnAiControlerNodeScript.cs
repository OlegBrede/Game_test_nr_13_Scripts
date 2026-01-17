using Godot;
using System;

public partial class PawnAiControlerNodeScript : Node2D
{
    AI_StategyBotScript MyMaster;
    public void TriggerConfirmAction()
    {
        
    }
    /// <summary>
    /// To jest do tego by wyznaczać ścierzki dla AI dłuższe niżeli pozwalało by ograniczenie dystansu pionka.
    /// </summary>
    /// <param name="path"> trzeba dać tam GetCurrentNavigationPath </param>
    /// <param name="startPosition"> Start position , mówi samo za siebie</param>
    /// <param name="maxDistance"> tutaj wejdzie Movement Allowence Distance czy "MAD"</param>
    /// <returns>zwraca ostatni możliwy punkt ruchu na podstawie długości ścierzki agenta nawigacji</returns>
    public Vector2 GetReachablePointOnPath(Vector2[] path,Vector2 startPosition,float maxDistance)
    {
        if (path == null || path.Length == 0)
            return startPosition;

        float remainingDistance = maxDistance;
        Vector2 currentPos = startPosition;

        foreach (var nextPoint in path)
        {
            float segmentLength = currentPos.DistanceTo(nextPoint);
            // Cały segment się mieści
            if (segmentLength <= remainingDistance)
            {
                remainingDistance -= segmentLength;
                currentPos = nextPoint;
            }
            else
            {
                // Jesteśmy w środku segmentu
                float t = remainingDistance / segmentLength;
                return currentPos.Lerp(nextPoint, t);
            }
        }
        // Jeśli cała ścieżka krótsza niż limit
        return currentPos;
    }
    //Na sto procent potrzebny będzie jakiś unit info parser by Nadrzędne AI wiedziało co z tym zrobić, niektóre przekładnie co do tego co było w kontrolerze dla gacza
    // czego tu NIE MA być na 100 % to decyzjoróbstwa od strony AI to mają być jedyie jego narzędzia, więc 
}
