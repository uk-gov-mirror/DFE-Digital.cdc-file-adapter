﻿namespace Dfe.CdcFileAdapter.FunctionApp.UnitTests.Functions
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Dfe.CdcFileAdapter.Application.Definitions;
    using Dfe.CdcFileAdapter.Domain.Definitions;
    using Dfe.CdcFileAdapter.Domain.Models;
    using Dfe.CdcFileAdapter.FunctionApp.Functions;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Http.Internal;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Primitives;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class GetFileTests
    {
        private Mock<IFileManager> mockFileManager;

        private GetFile getFile;

        [TestInitialize]
        public void Arrange()
        {
            this.mockFileManager = new Mock<IFileManager>();

            Mock<ILoggerProvider> mockLoggerProvider = new Mock<ILoggerProvider>();

            this.getFile = new GetFile(
                this.mockFileManager.Object,
                mockLoggerProvider.Object);
        }

        [TestMethod]
        public async Task RunAsync_NoHttpRequestProvided_ThrowsArgumentNullException()
        {
            // Arrange
            HttpRequest httpRequest = null;
            int urn = 1234;
            CancellationToken cancellationToken = CancellationToken.None;

            // Assert
            await Assert.ThrowsExceptionAsync<ArgumentNullException>(
                async () =>
                {
                    // Act
                    await this.getFile.RunAsync(
                        httpRequest,
                        urn,
                        cancellationToken);
                });
        }

        [TestMethod]
        public async Task RunAsync_NoTypeQueryProvided_ReturnsBadRequestResult()
        {
            // Arrange
            Mock<HttpRequest> mockHttpRequest = new Mock<HttpRequest>();

            QueryCollection queryCollection = new QueryCollection();
            mockHttpRequest.Setup(x => x.Query).Returns(queryCollection);

            HttpRequest httpRequest = mockHttpRequest.Object;
            int urn = 1234;
            CancellationToken cancellationToken = CancellationToken.None;

            IActionResult actionResult = null;

            // Act
            actionResult = await this.getFile.RunAsync(
                httpRequest,
                urn,
                cancellationToken).ConfigureAwait(false);

            // Assert
            Assert.IsInstanceOfType(actionResult, typeof(BadRequestResult));
        }

        [TestMethod]
        public async Task RunAsync_TypeQueryProvidedButInvalid_ReturnsBadeRequestResult()
        {
            // Arrange
            Mock<HttpRequest> mockHttpRequest = new Mock<HttpRequest>();

            Dictionary<string, StringValues> store =
                new Dictionary<string, StringValues>()
                {
                    { "type", new StringValues("some-rubbish") }
                };

            QueryCollection queryCollection = new QueryCollection(store);
            mockHttpRequest.Setup(x => x.Query).Returns(queryCollection);

            HttpRequest httpRequest = mockHttpRequest.Object;
            int urn = 1234;
            CancellationToken cancellationToken = CancellationToken.None;

            IActionResult actionResult = null;

            // Act
            actionResult = await this.getFile.RunAsync(
                httpRequest,
                urn,
                cancellationToken).ConfigureAwait(false);

            // Assert
            Assert.IsInstanceOfType(actionResult, typeof(BadRequestResult));
        }

        [TestMethod]
        public async Task RunAsync_FileDoesntExistInStorage_ReturnsNotFoundResult()
        {
            // Arrange
            Mock<HttpRequest> mockHttpRequest = new Mock<HttpRequest>();

            Dictionary<string, StringValues> store =
                new Dictionary<string, StringValues>()
                {
                    { "type", new StringValues("report") }
                };

            QueryCollection queryCollection = new QueryCollection(store);
            mockHttpRequest.Setup(x => x.Query).Returns(queryCollection);

            HttpRequest httpRequest = mockHttpRequest.Object;
            int urn = 1234;
            CancellationToken cancellationToken = CancellationToken.None;

            IActionResult actionResult = null;

            // Act
            actionResult = await this.getFile.RunAsync(
                httpRequest,
                urn,
                cancellationToken).ConfigureAwait(false);

            // Assert
            Assert.IsInstanceOfType(actionResult, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task RunAsync_FileExistsInStorage_ReturnsFileContentResult()
        {
            // Arrange
            byte[] contentBytes = Convert.FromBase64String(
                "VGhpcyBpcyBhIHRlc3QgdGV4dCBmaWxlLg==");

            File file = new File()
            {
                ContentType = "text/plain",
                ContentBytes = contentBytes,
            };

            this.mockFileManager
                .Setup(x => x.GetFileAsync(It.IsAny<int>(), It.IsAny<FileTypeOption>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(file));

            Mock<HttpRequest> mockHttpRequest = new Mock<HttpRequest>();

            Dictionary<string, StringValues> store =
                new Dictionary<string, StringValues>()
                {
                    { "type", new StringValues("site-plan") }
                };

            QueryCollection queryCollection = new QueryCollection(store);
            mockHttpRequest.Setup(x => x.Query).Returns(queryCollection);

            HttpRequest httpRequest = mockHttpRequest.Object;
            int urn = 1234;
            CancellationToken cancellationToken = CancellationToken.None;

            IActionResult actionResult = null;

            // Act
            actionResult = await this.getFile.RunAsync(
                httpRequest,
                urn,
                cancellationToken).ConfigureAwait(false);

            // Assert
            Assert.IsInstanceOfType(actionResult, typeof(FileContentResult));
        }
    }
}