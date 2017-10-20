using System.Collections.Generic;
using System.Linq;
using Structurizr;

namespace OpenSoftware.Structurizr.Dgml
{
    public static class StructurizrExtensions
    {
        /// <summary>
        /// Extension method that merges a collection of element styles into a single element style.
        /// </summary>
        /// <param name="matchingStyles"></param>
        /// <returns></returns>
        public  static ElementStyle Join(this IEnumerable<ElementStyle> matchingStyles)
        {
            var style = new ElementStyle("joinedStyle");
            foreach (var matchingStyle in matchingStyles.Reverse())
            {
                if (matchingStyle.Background != null)
                    style.Background = matchingStyle.Background;
                if (matchingStyle.Color != null) style.Color = matchingStyle.Color;
                if (matchingStyle.FontSize != null) style.FontSize = matchingStyle.FontSize;
                if (matchingStyle.Height != null) style.Height = matchingStyle.Height;
                if (matchingStyle.Shape != Shape.Box) style.Shape = matchingStyle.Shape;
                if (matchingStyle.Width != null) style.Width = matchingStyle.Width;
            }
            return style;
        }

        /// <summary>
        /// Method that checks if an element is a Structurizr container
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public static bool IsContainer(this Element element)
        {
            switch (element)
            {
                case SoftwareSystem system:
                    var sws = system;
                    return sws.Containers.Any();
                case Container container:
                    var c = container;
                    return c.Components.Any();
            }
            return false;
        }
    }
}