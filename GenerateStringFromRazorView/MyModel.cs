namespace GenerateStringFromRazorView
{
    public class MyModel
    {
        public MyModel(string label)
        {
            this.Label = label;
        }

        public string Label { get; }
    }
}
