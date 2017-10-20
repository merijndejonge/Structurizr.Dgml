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
        public static DirectedGraph ToDgml(this Workspace workspace)
        {

            var builder = new DgmlBuilder
            {
                NodeBuilders = new NodeBuilder[]
                {
                    new NodeBuilder<ElementView>(MakeNode)
                },
                LinkBuilders = new LinkBuilder[]
                {
                    new LinkBuilder<RelationshipView>(
                        x =>
                            new Link
                            {
                                Source = x.Relationship.SourceId,
                                Target = x.Relationship.DestinationId,
                                Description = x.Relationship.Description
                            }
                    )
                },
                CategoryBuilders = new CategoryBuilder[]
                {
                    new CategoryBuilder<ElementView>(BuildCategory)
                },
                StyleBuilders = new List<StyleBuilder>
                {
                    new StyleBuilder<Node>(x => CreateStyleForNode(workspace, x))
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

            return new Style
            {
                GroupLabel = categories[0],
                Setter = new List<Setter> {new Setter {Property = "Background", Value = style?.Background ?? "White"}}
            };
        }


        private static Category BuildCategory(ElementView elementView)
        {
            var element = elementView.Element;
            if (element.Parent == null) return null;
            return new Category {Id = element.Parent.Id, Label = element.Parent.Name};
        }

        private static Node MakeNode(ElementView elementView)
        {
            var element = elementView.Element;
            var labels = element.getRequiredTags();
            labels.Reverse();
            var categoryRefs = labels.Select(x => new CategoryRef {Ref = x}).ToList();
            if (element.Parent != null)
            {
                categoryRefs.Insert(0, new CategoryRef
                {
                    Ref = element.Parent.Id
                });
            }

            return new Node
            {
                Id = element.Id,
                Group = element.IsContainer() ? "Expanded" : null,
                Label = element.Name,
                Category = categoryRefs.Select(x => x.Ref).FirstOrDefault(),
                CategoryRefs = categoryRefs
            };
        }
    }
}