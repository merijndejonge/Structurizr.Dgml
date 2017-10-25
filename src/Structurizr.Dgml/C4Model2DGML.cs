using System.Collections.Generic;
using System.Linq;
using OpenSoftware.DgmlTools;
using OpenSoftware.DgmlTools.Builders;
using OpenSoftware.DgmlTools.Model;
using Structurizr;

namespace OpenSoftware.Structurizr.Dgml
{
    public static class C4Model2Dgml
    {
        /// <summary>
        /// Max length of a label strings
        /// </summary>
        private const int MaxLabelLength = 20;
        /// <summary>
        /// Url to shape pictures
        /// </summary>
        private const string ShapesUri =
            @"https://raw.githubusercontent.com/merijndejonge/Structurizr.Dgml/master/src/Structurizr.Dgml/Shapes";
        /// <summary>
        /// Extension method that converts a Structurizr C4 model to DGML.
        /// </summary>
        /// <param name="workspace"></param>
        /// <returns></returns>
        public static DirectedGraph ToDgml(this Workspace workspace)
        {

            var builder = new DgmlBuilder
            {
                NodeBuilders = new NodeBuilder[]
                {
                    new NodeBuilder<ElementView>(CreateNode)
                },
                LinkBuilders = new LinkBuilder[]
                {
                    new LinkBuilder<RelationshipView>(CreateLink)
                },
                CategoryBuilders = new CategoryBuilder[]
                {
                    new CategoryBuilder<ElementView>(CreateCategory)
                },
                StyleBuilders = new List<StyleBuilder>
                {
                    new StyleBuilder<Node>(x => CreateStyleForNode(workspace, x)),
                }
            };
            var contextElements = workspace.Views.SystemContextViews.SelectMany(x => x.Elements).Distinct().ToArray();
            var contextLinks = workspace.Views.SystemContextViews.SelectMany(x => x.Relationships).Distinct().ToArray();
            var containerElements = workspace.Views.ContainerViews.SelectMany(x => x.Elements).Distinct().ToArray();
            var containerLinks = workspace.Views.ContainerViews.SelectMany(x => x.Relationships).Distinct().ToArray();
            var componentElements = workspace.Views.ComponentViews.SelectMany(x => x.Elements).Distinct().ToArray();
            var componentLinks = workspace.Views.ComponentViews.SelectMany(x => x.Relationships).Distinct().ToArray();

            var graph = builder.Build(
                contextElements,
                contextLinks,
                containerElements,
                containerLinks,
                componentElements,
                componentLinks);
            return graph;
        }
        private static Node CreateNode(ElementView elementView)
        {
            var element = elementView.Element;
            var labels = element.GetTags();
            var categoryRefs = labels.Select(x => new CategoryRef { Ref = x }).ToList();
            if (element.Parent != null)
            {
                categoryRefs.Insert(0, new CategoryRef
                {
                    Ref = element.Parent.Id
                });
            }
            categoryRefs.Reverse();
            return new Node
            {
                Id = element.Id,
                Group = element.IsContainer() ? "Expanded" : null,
                Label = element.Name,
                Description = string.IsNullOrEmpty(element.Description) ? null : element.Description,
                Reference = element.Url,
                Category = categoryRefs.Select(x => x.Ref).FirstOrDefault(),
                CategoryRefs = categoryRefs
            };
        }
        private static Link CreateLink(RelationshipView relationship)
        {
            var link = new Link
            {
                Source = relationship.Relationship.SourceId,
                Target = relationship.Relationship.DestinationId,
            };
            if (string.IsNullOrWhiteSpace(relationship.Relationship.Description) == false)
            {
                link.Description = relationship.Relationship.Description;
                link.Label = MakeLabel(relationship.Relationship.Description);
            }
            return link;
        }
        private static Category CreateCategory(ElementView elementView)
        {
            var element = elementView.Element;
            if (element.Parent == null) return null;
            return new Category { Id = element.Parent.Id, Label = element.Parent.Name };
        }
        private static string MakeLabel(string relationshipDescription)
        {
            if (relationshipDescription.Length < MaxLabelLength) return relationshipDescription;
            return relationshipDescription.Substring(0, MaxLabelLength - 3) + "...";
        }
        private static Style CreateStyleForNode(Workspace workspace, Node node)
        {
            if (node.Category == null) return null;

            var categories = node.CategoryRefs.Select(x => x.Ref).ToArray();
            var styles = workspace.Views.Configuration.Styles;
            var element =
                workspace.Model.Relationships.Where(x => x.SourceId == node.Category)
                    .Select(x => x.Source)
                    .Distinct()
                    .SingleOrDefault();
            if (element != null)
            {
                categories = categories.Skip(1).ToArray();
            }

            var matchingStyles = styles.Elements.Where(x => categories.Any(tag => x.Tag == tag));
            var style = matchingStyles.Join();

            return Element2DgmlStyle(categories[0], style);
        }
        private static Style Element2DgmlStyle(string label, ElementStyle style)
        {
            return new Style
            {
                TargetType = "Node",
                GroupLabel = label,
                Condition = new List<Condition>
                {
                    new Condition{Expression = $"HasCategory('{label}')"}
                },
                Setter = StyleElements2Dgml(style).ToList()
            };
        }
        private static IEnumerable<Setter> StyleElements2Dgml(ElementStyle style)
        {
            if (style.Color != null)
            {
                yield return new Setter {Property = "Foreground", Value = style.Color};
            }
            if (style.Background != null)
            {
                yield return new Setter { Property = "Background", Value = style.Background };
            }
            if (style.Shape != Shape.Box)
            {
                yield return new Setter { Property = "Shape", Value = "None" };
                yield return new Setter { Property = "Icon", Value =  $"{ShapesUri}/{style.Shape.ToString()}.png"};
            }
        }
    }
}