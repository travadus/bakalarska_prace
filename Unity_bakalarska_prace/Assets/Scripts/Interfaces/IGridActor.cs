public interface IGridActor
{
    // FÁZE 1: PØÍTOK (Kolik mùeš dodat do sítì?)
    // Vrací mnoství energie (MWh), kterou budova TEÏ nabízí síti.
    float GetAvailableSupply();

    // Pokud si sí energii vezme, zavolá tuto metodu, aby ji budova odeèetla.
    void ExtractEnergy(float amount);


    // FÁZE 2: ODBÌR (Kolik chceš ze sítì?)
    // Vrací mnoství energie (MWh), kterou budova TEÏ potøebuje.
    float GetRequestedDemand();

    // Sí pošle energii budovì (mùe poslat ménì, ne budova chtìla, pokud je nedostatek).
    void ReceiveEnergy(float amount);
}