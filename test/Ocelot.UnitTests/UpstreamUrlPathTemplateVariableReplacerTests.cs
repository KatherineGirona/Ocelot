using System.Collections.Generic;
using Ocelot.Library.Infrastructure.UrlMatcher;
using Ocelot.Library.Infrastructure.UrlTemplateReplacer;
using Shouldly;
using Xunit;

namespace Ocelot.UnitTests
{
    using TestStack.BDDfy;

    public class UpstreamUrlPathTemplateVariableReplacerTests
    {
        private string _downstreamUrlTemplate;
        private UrlMatch _urlMatch;
        private string _result;
        private readonly IDownstreamUrlTemplateVariableReplacer _downstreamUrlPathReplacer;

        public UpstreamUrlPathTemplateVariableReplacerTests()
        {
            _downstreamUrlPathReplacer = new DownstreamUrlTemplateVariableReplacer();
        }

        [Fact]
        public void can_replace_no_template_variables()
        {
            this.Given(x => x.GivenThereIsADownstreamUrl(""))
                .And(x => x.GivenThereIsAUrlMatch(new UrlMatch(true, new List<TemplateVariableNameAndValue>(), "")))
                .When(x => x.WhenIReplaceTheTemplateVariables())
                .Then(x => x.ThenTheDownstreamUrlPathIsReturned(""))
                .BDDfy();
        }

        [Fact]
        public void can_replace_url_no_slash()
        {
            this.Given(x => x.GivenThereIsADownstreamUrl("api"))
                .And(x => x.GivenThereIsAUrlMatch(new UrlMatch(true, new List<TemplateVariableNameAndValue>(), "api")))
                .When(x => x.WhenIReplaceTheTemplateVariables())
                .Then(x => x.ThenTheDownstreamUrlPathIsReturned("api"))
                .BDDfy();
        }

        [Fact]
        public void can_replace_url_one_slash()
        {
            this.Given(x => x.GivenThereIsADownstreamUrl("api/"))
                .And(x => x.GivenThereIsAUrlMatch(new UrlMatch(true, new List<TemplateVariableNameAndValue>(), "api/")))
                .When(x => x.WhenIReplaceTheTemplateVariables())
                .Then(x => x.ThenTheDownstreamUrlPathIsReturned("api/"))
                .BDDfy();
        }

        [Fact]
        public void can_replace_url_multiple_slash()
        {
            this.Given(x => x.GivenThereIsADownstreamUrl("api/product/products/"))
                .And(x => x.GivenThereIsAUrlMatch(new UrlMatch(true, new List<TemplateVariableNameAndValue>(), "api/product/products/")))
                .When(x => x.WhenIReplaceTheTemplateVariables())
                .Then(x => x.ThenTheDownstreamUrlPathIsReturned("api/product/products/"))
                .BDDfy();
        }

        [Fact]
        public void can_replace_url_one_template_variable()
        {
            var templateVariables = new List<TemplateVariableNameAndValue>()
            {
                new TemplateVariableNameAndValue("{productId}", "1")
            };

            this.Given(x => x.GivenThereIsADownstreamUrl("productservice/products/{productId}/"))
             .And(x => x.GivenThereIsAUrlMatch(new UrlMatch(true, templateVariables, "api/products/{productId}/")))
             .When(x => x.WhenIReplaceTheTemplateVariables())
             .Then(x => x.ThenTheDownstreamUrlPathIsReturned("productservice/products/1/"))
             .BDDfy();
        }

        [Fact]
        public void can_replace_url_one_template_variable_with_path_after()
        {
            var templateVariables = new List<TemplateVariableNameAndValue>()
            {
                new TemplateVariableNameAndValue("{productId}", "1")
            };

            this.Given(x => x.GivenThereIsADownstreamUrl("productservice/products/{productId}/variants"))
             .And(x => x.GivenThereIsAUrlMatch(new UrlMatch(true, templateVariables, "api/products/{productId}/")))
             .When(x => x.WhenIReplaceTheTemplateVariables())
             .Then(x => x.ThenTheDownstreamUrlPathIsReturned("productservice/products/1/variants"))
             .BDDfy();
        }

        [Fact]
        public void can_replace_url_two_template_variable()
        {
            var templateVariables = new List<TemplateVariableNameAndValue>()
            {
                new TemplateVariableNameAndValue("{productId}", "1"),
                new TemplateVariableNameAndValue("{variantId}", "12")
            };

            this.Given(x => x.GivenThereIsADownstreamUrl("productservice/products/{productId}/variants/{variantId}"))
             .And(x => x.GivenThereIsAUrlMatch(new UrlMatch(true, templateVariables, "api/products/{productId}/{variantId}")))
             .When(x => x.WhenIReplaceTheTemplateVariables())
             .Then(x => x.ThenTheDownstreamUrlPathIsReturned("productservice/products/1/variants/12"))
             .BDDfy();
        }

           [Fact]
        public void can_replace_url_three_template_variable()
        {
            var templateVariables = new List<TemplateVariableNameAndValue>()
            {
                new TemplateVariableNameAndValue("{productId}", "1"),
                new TemplateVariableNameAndValue("{variantId}", "12"),
                new TemplateVariableNameAndValue("{categoryId}", "34")
            };

            this.Given(x => x.GivenThereIsADownstreamUrl("productservice/category/{categoryId}/products/{productId}/variants/{variantId}"))
             .And(x => x.GivenThereIsAUrlMatch(new UrlMatch(true, templateVariables, "api/products/{categoryId}/{productId}/{variantId}")))
             .When(x => x.WhenIReplaceTheTemplateVariables())
             .Then(x => x.ThenTheDownstreamUrlPathIsReturned("productservice/category/34/products/1/variants/12"))
             .BDDfy();
        }

        private void GivenThereIsADownstreamUrl(string downstreamUrlTemplate)
        {
            _downstreamUrlTemplate = downstreamUrlTemplate;
        }

        private void GivenThereIsAUrlMatch(UrlMatch urlMatch)
        {
            _urlMatch = urlMatch;
        }

        private void WhenIReplaceTheTemplateVariables()
        {
            _result = _downstreamUrlPathReplacer.ReplaceTemplateVariable(_downstreamUrlTemplate, _urlMatch);
        }

        private void ThenTheDownstreamUrlPathIsReturned(string expected)
        {
            _result.ShouldBe(expected);
        }

    }
}
