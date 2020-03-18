using DotEntity;
using DotEntity.Versioning;
using EvenCart.Data.Entity.Users;
using Support.Forums.Data;
using Db = DotEntity.DotEntity.Database;
namespace Support.Forums.Versions
{
    public class Version_1_0 : IDatabaseVersion
    {
        public void Upgrade(IDotEntityTransaction transaction)
        {
            Db.CreateTable<ForumGroup>(transaction);
            Db.CreateTable<Forum>(transaction);
            Db.CreateTable<ForumThread>(transaction);
            Db.CreateTable<ForumPost>(transaction);
            Db.CreateTable<ForumVote>(transaction);

            Db.CreateConstraint(Relation.Create<User, ForumPost>("Id", "UserId"), transaction, false);
            Db.CreateConstraint(Relation.Create<User, ForumThread>("Id", "UserId"), transaction, true);
            Db.CreateConstraint(Relation.Create<User, ForumVote>("Id", "UserId"), transaction, false);

            Db.CreateConstraint(Relation.Create<ForumGroup, Forum>("Id", "ForumGroupId"), transaction, true);
            Db.CreateConstraint(Relation.Create<Forum, ForumThread>("Id", "ForumId"), transaction, true);
            Db.CreateConstraint(Relation.Create<ForumThread, ForumPost>("Id", "ForumThreadId"), transaction, true);
            Db.CreateConstraint(Relation.Create<ForumThread, ForumVote>("Id", "ForumThreadId"), transaction, true);

            Db.CreateIndex<ForumPost>(new[] {nameof(ForumPost.CreatedOn)}, transaction);
            Db.CreateIndex<ForumThread>(new[] { nameof(ForumThread.CreatedOn) }, transaction);
        }

        public void Downgrade(IDotEntityTransaction transaction)
        {
            Db.DropIndex<ForumPost>(new[] { nameof(ForumPost.CreatedOn) }, transaction);
            Db.DropIndex<ForumThread>(new[] { nameof(ForumThread.CreatedOn) }, transaction);

            Db.DropConstraint(Relation.Create<User, ForumPost>("Id", "UserId"), transaction);
            Db.DropConstraint(Relation.Create<User, ForumThread>("Id", "UserId"), transaction);
            Db.DropConstraint(Relation.Create<User, ForumVote>("Id", "UserId"), transaction);

            Db.DropConstraint(Relation.Create<ForumThread, ForumPost>("Id", "ForumThreadId"), transaction);
            Db.DropConstraint(Relation.Create<Forum, ForumThread>("Id", "ForumId"), transaction);
            Db.DropConstraint(Relation.Create<ForumThread, ForumVote>("Id", "ForumThreadId"), transaction);
            Db.DropConstraint(Relation.Create<ForumGroup, Forum>("Id", "ForumGroupId"), transaction);

            Db.DropTable<ForumGroup>(transaction);
            Db.DropTable<Forum>(transaction);
            Db.DropTable<ForumThread>(transaction);
            Db.DropTable<ForumPost>(transaction);
            Db.DropTable<ForumVote>(transaction);
        }

        public string VersionKey { get; } = "Support.Forums.Versions.Version_1_0";
    }
}