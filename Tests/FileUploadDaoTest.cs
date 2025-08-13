
using Moq;
using Microsoft.EntityFrameworkCore;
using Internal_API.data;
using Internal_API.models;

namespace Internal_API.Tests
{
    [TestClass]
    public class FileUploadDaoTests
    {
        private Mock<AppDbContext> mockDbContext;
        private Mock<DbSet<FileUpload>> mockDbSet;
        private FileUploadDao fileUploadDao;
        private List<FileUpload> fileUploads;
        private int year;

        [TestInitialize]
        public void Setup()
        {

            year = 2023;
            var options = new DbContextOptions<AppDbContext>();
            mockDbContext = new Mock<AppDbContext>(options);
            fileUploadDao = new FileUploadDao(mockDbContext.Object);

            fileUploads = new List<FileUpload>
        {
            new FileUpload { id = Guid.NewGuid().ToString(), filename = "file1.pdf", year = 2023 },
            new FileUpload { id = Guid.NewGuid().ToString(), filename = "file2.pdf", year = 2023 },
            new FileUpload { id = Guid.NewGuid().ToString(), filename = "file3.pdf", year = 2024 }
        };

            mockDbSet = new Mock<DbSet<FileUpload>>();
            mockDbSet.As<IQueryable<FileUpload>>().Setup(m => m.Provider).Returns(fileUploads.AsQueryable().Provider);
            mockDbSet.As<IQueryable<FileUpload>>().Setup(m => m.Expression).Returns(fileUploads.AsQueryable().Expression);
            mockDbSet.As<IQueryable<FileUpload>>().Setup(m => m.ElementType).Returns(fileUploads.AsQueryable().ElementType);
            mockDbSet.As<IQueryable<FileUpload>>().Setup(m => m.GetEnumerator()).Returns(fileUploads.GetEnumerator());

            mockDbContext.Setup(db => db.FileUploads).Returns(mockDbSet.Object);

        }

        [TestMethod]
        public void GetListUploads_ReturnsListOfUploadsForGivenYear()
        {

            // Act
            var result = fileUploadDao.GetListUploadsByYear(year);

            // Assert
            Assert.IsNotNull(result); // Expect a non-null list
            Assert.AreEqual(2, result.Count); // Expect 2 records
            Assert.IsTrue(result.All(u => u.year == year)); // All records should match the year
        }

        [TestMethod]
        public void GetUploadById_ShouldReturnCorrectFileUpload()
        {
            var expectedFile = fileUploads[0];
            var result = fileUploadDao.GetUploadById(expectedFile.id);

            Assert.IsNotNull(result);
            Assert.AreEqual(expectedFile.id, result.id);
        }

        [TestMethod]
        public void SaveFileUpload_ShouldAddFileUploadToDatabase()
        {
            var fileInfo = new S3FileInfo { fileName = "file4.pdf", category = "docs", s3key = "s3key", userInfo = "user1", year = 2025 };
            mockDbSet.Setup(m => m.Add(It.IsAny<FileUpload>())).Callback<FileUpload>(f => fileUploads.Add(f));

            var result = fileUploadDao.SaveFileUpload(fileInfo);

            Assert.AreEqual(4, fileUploads.Count);
            Assert.IsTrue(fileUploads.Any(f => f.filename == "file4.pdf"));
        }
    }
}
