namespace Cauldron.Shared.MessagePackObjects.Value
{
    public static class AnnotationsModifierExtensions
    {
        public static string[] Modify(this AnnotationsModifier _this, IReadOnlyList<string> value)
        {
            return _this.Operator switch
            {
                AnnotationsModifier.OperatorValue.Add => value.Concat(_this.Value).Distinct().ToArray(),
                AnnotationsModifier.OperatorValue.Remove => value.Where(x => !_this.Value.Contains(x)).ToArray(),
                AnnotationsModifier.OperatorValue.Clear => Array.Empty<string>(),
                _ => throw new InvalidOperationException()
            };
        }
    }
}
