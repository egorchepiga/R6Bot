namespace Segmentation.Algorithms.Helpers
{
    public class InteractionItemBase<TSettings, TObservations>
    {
        public TSettings Settings { get; set; }
        public TObservations Observations { get; set; }

        public InteractionItemBase(TSettings settings, TObservations observations)
        {
            Settings = settings;
            Observations = observations;
        }
    }
}
