using System;
using System.Text.Json;
using Xunit;

namespace dsi.tests
{
    public static class ObjectAssertExtension
    {
        public static void AssertWith(this object obj, object anotherObj)
        {
            if (ReferenceEquals(obj, anotherObj)) return;

            if (obj == null || anotherObj == null)
            {
                throw new ArgumentNullException(nameof(obj));
            }

            if (obj.GetType() != anotherObj.GetType())
            {
                throw new ArgumentException(nameof(obj), nameof(anotherObj));
            }

            var objJson = obj as string ?? JsonSerializer.Serialize(obj);
            var anotherJson = anotherObj as string ?? JsonSerializer.Serialize(anotherObj);

            Assert.Equal(objJson, anotherJson, ignoreLineEndingDifferences: true);
        }

    }
}
