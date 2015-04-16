﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Nancy;
using Nancy.SimpleAuthentication;
using Nancy.SimpleAuthentication.Testing;
using Nancy.Testing;
using Shouldly;
using SimpleAuthentication.Core;
using SimpleAuthentication.Core.Exceptions;
using Xunit;

namespace SimpleAuthentication.Tests.WebSites.Nancy
{
    public class FakeSimpleAuthenticationModuleFacts
    {
        public class RedirectToProviderFacts
        {
            [Fact]
            public void GivenAGetRequestWithAValidProvider_RedirectToProvider_ReturnsASeeOtherRedirectResult()
            {
                // Arrange.
                
                // This is some fake information that is returned from a provider.
                var authenticationProviderCallback = new SampleJsonAuthenticationProviderCallback();

                var browser = new Browser(with =>
                {
                    with.Module<FakeSimpleAuthenticationModule>();
                    with.Dependency<IAuthenticationProviderCallback>(authenticationProviderCallback);
                    with.Dependency(new AuthenticateCallbackResult());
                });

                // Act.
                var result = browser.Get("/authenticate/google");

                // Assert.
                result.StatusCode.ShouldBe(HttpStatusCode.SeeOther);
                result.Headers["location"].ShouldBe("http://www.someProvider.com/oauth/authenticate");
            }
        }

        public class AuthenticateCallbackFacts
        {
            [Fact]
            public void GivenAGetRequestFromAProvider_AuthenticateCallback_ReturnsAJsonResult()
            {
                // Arrange.
                // This is some fake information that is returned from a provider.
                var accessToken = new AccessToken
                {
                    Token = "abcde",
                    Secret = "pewpewpew",
                    ExpiresOn = new DateTime(2020, 5, 23)
                };
                var userInformation = new UserInformation
                {
                    Name = "Pure Krome",
                    Email = "pewpew@some.email.rebell-alliance"
                };
                const string rawUserInformation = "some json string with raw user information";

                var authenticatedClient = new AuthenticatedClient("google",
                    accessToken,
                    userInformation,
                    rawUserInformation);

                // Finally, the full result.
                var authenticateCallbackResult = new AuthenticateCallbackResult()
                {
                    AuthenticatedClient = authenticatedClient
                };

                var browser = new Browser(with =>
                {
                    with.Module<FakeSimpleAuthenticationModule>();
                    with.Dependency<IAuthenticationProviderCallback>(new SampleJsonAuthenticationProviderCallback());
                    with.Dependency(authenticateCallbackResult);
                });

                // Act.
                var result = browser.Get("/authenticate/callback");

                // Assert.
                result.StatusCode.ShouldBe(HttpStatusCode.OK);
            }

            [Fact]
            public void GivenAGetRequestFromAProviderWithAReturnUrl_AuthenticateCallback_ReturnsAJsonResult()
            {
                // Arrange.
                // This is some fake information that is returned from a provider.
                var accessToken = new AccessToken
                {
                    Token = "abcde",
                    Secret = "pewpewpew",
                    ExpiresOn = new DateTime(2020, 5, 23)
                };
                var userInformation = new UserInformation
                {
                    Name = "Pure Krome",
                    Email = "pewpew@some.email.rebell-alliance"
                };
                const string rawUserInformation = "some json string with raw user information";

                var authenticatedClient = new AuthenticatedClient("google",
                    accessToken,
                    userInformation,
                    rawUserInformation);

                // Finally, the full result.
                var authenticateCallbackResult = new AuthenticateCallbackResult()
                {
                    AuthenticatedClient = authenticatedClient,
                    ReturnUrl = "/"
                };

                var browser = new Browser(with =>
                {
                    with.Module<FakeSimpleAuthenticationModule>();
                    with.Dependency<IAuthenticationProviderCallback>(new SampleJsonAuthenticationProviderCallback());
                    with.Dependency(authenticateCallbackResult);
                });

                // Act.
                var result = browser.Get("/authenticate/callback");

                // Assert.
                result.StatusCode.ShouldBe(HttpStatusCode.SeeOther);
            }

            [Fact]
            public void GivenAnErrorResponseFromAProvider_AuthenticateCallback_ReturnsAnError()
            {
                // Arrange.
                var authenticationException = new AuthenticationException("Some error occured at the provuder.");
                var browser = new Browser(with =>
                {
                    var module = new FakeSimpleAuthenticationModule(new SampleJsonAuthenticationProviderCallback(),
                        authenticationException);
                    with.Module(module);
                });

                // Act.
                var result = browser.Get("/authenticate/callback");

                // Assert.
                result.StatusCode.ShouldBe(HttpStatusCode.InternalServerError);
            }
        }
    }
}