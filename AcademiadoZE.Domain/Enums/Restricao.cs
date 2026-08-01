namespace AcademiadoZE.Domain.Enums;
//Mario Cesar alves Júnior
[Flags]
public enum Restricao

{
    None = 0,
    Diabetes = 1,
    PressaoAlta = 2,
    Labirintite = 4,
    Alergias = 8,
    ProblemasRespiratorios = 16,
    RemedioContinuo = 32
}