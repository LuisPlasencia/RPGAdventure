namespace RPG.Control
{
    public enum CursorType   // siempre añadir nuevos enum al final y no entre medias o al principio porque esto va por ints. Entonces si ponemos en la línea 2 donde está movement por ejemplo uno distinto, todo lo que era antes movement ahora es lo nuevo que pusimos 
    {
        None,
        Movement,
        Combat,
        UI,
        Pickup
    }
}
