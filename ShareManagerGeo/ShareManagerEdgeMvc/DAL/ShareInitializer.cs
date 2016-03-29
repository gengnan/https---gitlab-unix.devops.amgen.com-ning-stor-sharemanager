using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ShareManagerEdgeMvc.Models;
using System.Data.Entity;

namespace ShareManagerEdgeMvc.DAL
{
    public class ShareInitializer : System.Data.Entity.DropCreateDatabaseIfModelChanges<ShareContext>
    {

        protected override void Seed(ShareContext context)
        {
            var ous = new List<Ou>
            {
                new Ou{Domain=Domain.AM, OrganizationalUnit="OU=Groups,OU=Ent-Storage,OU=AM,OU=Amgen,DC=am,DC=corp,DC=amgen,DC=com", ResolverGroup="IS-STORAGE"},
                new Ou{Domain=Domain.EU, OrganizationalUnit="OU=Groups,OU=Ent-Storage,OU=EU,OU=Amgen,DC=eu,DC=corp,DC=amgen,DC=com", ResolverGroup="IS-STORAGE"},
                new Ou{Domain=Domain.AP, OrganizationalUnit="OU=Groups,OU=Ent-Storage,OU=AP,OU=Amgen,DC=ap,DC=corp,DC=amgen,DC=com", ResolverGroup="IS-STORAGE"}
            };
            ous.ForEach(s => context.Ous.Add(s));
            context.SaveChanges();

            var shares = new List<CifsShare>
            {
                new CifsShare{Name="Test01", UncPath="\\\\Test\\Test", CmdbCi="\\\\USWA-PFSX-CF03B\\alm_repositories", ShareOwnerFunctionalArea="EISTS", ShareOwnerCostCenter="1024", OwnerGroup="test-sharemgmt_GK", ReadOnlyGroup="test-sharemgmt_RO", ReadWriteGroup="test-sharemgmt_RW", NoChangeGroup="test-sharemgmt_NC", Status=Status.InService, OuID=1, CreatedOnDateTime=System.DateTime.Now, CreatedBy="AM\\keklein", ModifiedOnDateTime=System.DateTime.Now, ModifiedBy="AM\\keklein"},
                new CifsShare{Name="Test02", UncPath="\\\\Test\\Test", CmdbCi="\\\\USWA-PFSX-CF03B\\alm_repositories", ShareOwnerFunctionalArea="EISTS", ShareOwnerCostCenter="1024", OwnerGroup="test-sharemgmt_GK", ReadOnlyGroup="test-sharemgmt_RO", ReadWriteGroup="test-sharemgmt_RW", NoChangeGroup="test-sharemgmt_NC", Status=Status.InService, OuID=1, CreatedOnDateTime=System.DateTime.Now, CreatedBy="AM\\keklein", ModifiedOnDateTime=System.DateTime.Now, ModifiedBy="AM\\keklein"},
                new CifsShare{Name="Test03", UncPath="\\\\Test\\Test", CmdbCi="\\\\USWA-PFSX-CF03B\\alm_repositories", ShareOwnerFunctionalArea="EISTS", ShareOwnerCostCenter="1024", OwnerGroup="test-sharemgmt_GK", ReadOnlyGroup="test-sharemgmt_RO", ReadWriteGroup="test-sharemgmt_RW", NoChangeGroup="test-sharemgmt_NC", Status=Status.InService, OuID=1, CreatedOnDateTime=System.DateTime.Now, CreatedBy="AM\\keklein", ModifiedOnDateTime=System.DateTime.Now, ModifiedBy="AM\\keklein"},
                new CifsShare{Name="Test04", UncPath="\\\\Test\\Test", CmdbCi="\\\\USWA-PFSX-CF03B\\alm_repositories", ShareOwnerFunctionalArea="EISTS", ShareOwnerCostCenter="1024", OwnerGroup="test-sharemgmt_GK", ReadOnlyGroup="test-sharemgmt_RO", ReadWriteGroup="test-sharemgmt_RW", NoChangeGroup="test-sharemgmt_NC", Status=Status.InService, OuID=1, CreatedOnDateTime=System.DateTime.Now, CreatedBy="AM\\keklein", ModifiedOnDateTime=System.DateTime.Now, ModifiedBy="AM\\keklein"},
                new CifsShare{Name="Test05", UncPath="\\\\Test\\Test", CmdbCi="\\\\USWA-PFSX-CF03B\\alm_repositories", ShareOwnerFunctionalArea="EISTS", ShareOwnerCostCenter="1024", OwnerGroup="test-sharemgmt_GK", ReadOnlyGroup="test-sharemgmt_RO", ReadWriteGroup="test-sharemgmt_RW", NoChangeGroup="test-sharemgmt_NC", Status=Status.InService, OuID=1, CreatedOnDateTime=System.DateTime.Now, CreatedBy="AM\\keklein", ModifiedOnDateTime=System.DateTime.Now, ModifiedBy="AM\\keklein"},
                new CifsShare{Name="Test06", UncPath="\\\\Test\\Test", CmdbCi="\\\\USWA-PFSX-CF03B\\alm_repositories", ShareOwnerFunctionalArea="EISTS", ShareOwnerCostCenter="1024", OwnerGroup="test-sharemgmt_GK", ReadOnlyGroup="test-sharemgmt_RO", ReadWriteGroup="test-sharemgmt_RW", NoChangeGroup="test-sharemgmt_NC", Status=Status.InService, OuID=1, CreatedOnDateTime=System.DateTime.Now, CreatedBy="AM\\keklein", ModifiedOnDateTime=System.DateTime.Now, ModifiedBy="AM\\keklein"},
                new CifsShare{Name="Test07", UncPath="\\\\Test\\Test", CmdbCi="\\\\USWA-PFSX-CF03B\\alm_repositories", ShareOwnerFunctionalArea="EISTS", ShareOwnerCostCenter="1024", OwnerGroup="test-sharemgmt_GK", ReadOnlyGroup="test-sharemgmt_RO", ReadWriteGroup="test-sharemgmt_RW", NoChangeGroup="test-sharemgmt_NC", Status=Status.InService, OuID=1, CreatedOnDateTime=System.DateTime.Now, CreatedBy="AM\\keklein", ModifiedOnDateTime=System.DateTime.Now, ModifiedBy="AM\\keklein"},
                new CifsShare{Name="Test08", UncPath="\\\\Test\\Test", CmdbCi="\\\\USWA-PFSX-CF03B\\alm_repositories", ShareOwnerFunctionalArea="EISTS", ShareOwnerCostCenter="1024", OwnerGroup="test-sharemgmt_GK", ReadOnlyGroup="test-sharemgmt_RO", ReadWriteGroup="test-sharemgmt_RW", NoChangeGroup="test-sharemgmt_NC", Status=Status.InService, OuID=1, CreatedOnDateTime=System.DateTime.Now, CreatedBy="AM\\keklein", ModifiedOnDateTime=System.DateTime.Now, ModifiedBy="AM\\keklein"},
                new CifsShare{Name="Test09", UncPath="\\\\Test\\Test", CmdbCi="\\\\USWA-PFSX-CF03B\\alm_repositories", ShareOwnerFunctionalArea="EISTS", ShareOwnerCostCenter="1024", OwnerGroup="test-sharemgmt_GK", ReadOnlyGroup="test-sharemgmt_RO", ReadWriteGroup="test-sharemgmt_RW", NoChangeGroup="test-sharemgmt_NC", Status=Status.InService, OuID=1, CreatedOnDateTime=System.DateTime.Now, CreatedBy="AM\\keklein", ModifiedOnDateTime=System.DateTime.Now, ModifiedBy="AM\\keklein"},
                new CifsShare{Name="Test10", UncPath="\\\\Test\\Test", CmdbCi="\\\\USWA-PFSX-CF03B\\alm_repositories", ShareOwnerFunctionalArea="EISTS", ShareOwnerCostCenter="1024", OwnerGroup="test-sharemgmt_GK", ReadOnlyGroup="test-sharemgmt_RO", ReadWriteGroup="test-sharemgmt_RW", NoChangeGroup="test-sharemgmt_NC", Status=Status.InService, OuID=1, CreatedOnDateTime=System.DateTime.Now, CreatedBy="AM\\keklein", ModifiedOnDateTime=System.DateTime.Now, ModifiedBy="AM\\keklein"},
                new CifsShare{Name="Test11", UncPath="\\\\Test\\Test", CmdbCi="\\\\USWA-PFSX-CF03B\\alm_repositories", ShareOwnerFunctionalArea="EISTS", ShareOwnerCostCenter="1024", OwnerGroup="test-sharemgmt_GK", ReadOnlyGroup="test-sharemgmt_RO", ReadWriteGroup="test-sharemgmt_RW", NoChangeGroup="test-sharemgmt_NC", Status=Status.InService, OuID=1, CreatedOnDateTime=System.DateTime.Now, CreatedBy="AM\\keklein", ModifiedOnDateTime=System.DateTime.Now, ModifiedBy="AM\\keklein"},
                new CifsShare{Name="Test12", UncPath="\\\\Test\\Test", CmdbCi="\\\\USWA-PFSX-CF03B\\alm_repositories", ShareOwnerFunctionalArea="EISTS", ShareOwnerCostCenter="1024", OwnerGroup="test-sharemgmt_GK", ReadOnlyGroup="test-sharemgmt_RO", ReadWriteGroup="test-sharemgmt_RW", NoChangeGroup="test-sharemgmt_NC", Status=Status.InService, OuID=1, CreatedOnDateTime=System.DateTime.Now, CreatedBy="AM\\keklein", ModifiedOnDateTime=System.DateTime.Now, ModifiedBy="AM\\keklein"},
                new CifsShare{Name="Test13", UncPath="\\\\Test\\Test", CmdbCi="\\\\USWA-PFSX-CF03B\\alm_repositories", ShareOwnerFunctionalArea="EISTS", ShareOwnerCostCenter="1024", OwnerGroup="test-sharemgmt_GK", ReadOnlyGroup="test-sharemgmt_RO", ReadWriteGroup="test-sharemgmt_RW", NoChangeGroup="test-sharemgmt_NC", Status=Status.InService, OuID=1, CreatedOnDateTime=System.DateTime.Now, CreatedBy="AM\\keklein", ModifiedOnDateTime=System.DateTime.Now, ModifiedBy="AM\\keklein"},
                new CifsShare{Name="Test14", UncPath="\\\\Test\\Test", CmdbCi="\\\\USWA-PFSX-CF03B\\alm_repositories", ShareOwnerFunctionalArea="EISTS", ShareOwnerCostCenter="1024", OwnerGroup="test-sharemgmt_GK", ReadOnlyGroup="test-sharemgmt_RO", ReadWriteGroup="test-sharemgmt_RW", NoChangeGroup="test-sharemgmt_NC", Status=Status.InService, OuID=1, CreatedOnDateTime=System.DateTime.Now, CreatedBy="AM\\keklein", ModifiedOnDateTime=System.DateTime.Now, ModifiedBy="AM\\keklein"},
                new CifsShare{Name="Test15", UncPath="\\\\Test\\Test", CmdbCi="\\\\USWA-PFSX-CF03B\\alm_repositories", ShareOwnerFunctionalArea="EISTS", ShareOwnerCostCenter="1024", OwnerGroup="test-sharemgmt_GK", ReadOnlyGroup="test-sharemgmt_RO", ReadWriteGroup="test-sharemgmt_RW", NoChangeGroup="test-sharemgmt_NC", Status=Status.InService, OuID=1, CreatedOnDateTime=System.DateTime.Now, CreatedBy="AM\\keklein", ModifiedOnDateTime=System.DateTime.Now, ModifiedBy="AM\\keklein"},
                new CifsShare{Name="Test16", UncPath="\\\\Test\\Test", CmdbCi="\\\\USWA-PFSX-CF03B\\alm_repositories", ShareOwnerFunctionalArea="EISTS", ShareOwnerCostCenter="1024", OwnerGroup="test-sharemgmt_GK", ReadOnlyGroup="test-sharemgmt_RO", ReadWriteGroup="test-sharemgmt_RW", NoChangeGroup="test-sharemgmt_NC", Status=Status.InService, OuID=1, CreatedOnDateTime=System.DateTime.Now, CreatedBy="AM\\keklein", ModifiedOnDateTime=System.DateTime.Now, ModifiedBy="AM\\keklein"},
                new CifsShare{Name="Test17", UncPath="\\\\Test\\Test", CmdbCi="\\\\USWA-PFSX-CF03B\\alm_repositories", ShareOwnerFunctionalArea="EISTS", ShareOwnerCostCenter="1024", OwnerGroup="test-sharemgmt_GK", ReadOnlyGroup="test-sharemgmt_RO", ReadWriteGroup="test-sharemgmt_RW", NoChangeGroup="test-sharemgmt_NC", Status=Status.InService, OuID=1, CreatedOnDateTime=System.DateTime.Now, CreatedBy="AM\\keklein", ModifiedOnDateTime=System.DateTime.Now, ModifiedBy="AM\\keklein"},
                new CifsShare{Name="Test18", UncPath="\\\\Test\\Test", CmdbCi="\\\\USWA-PFSX-CF03B\\alm_repositories", ShareOwnerFunctionalArea="EISTS", ShareOwnerCostCenter="1024", OwnerGroup="test-sharemgmt_GK", ReadOnlyGroup="test-sharemgmt_RO", ReadWriteGroup="test-sharemgmt_RW", NoChangeGroup="test-sharemgmt_NC", Status=Status.InService, OuID=1, CreatedOnDateTime=System.DateTime.Now, CreatedBy="AM\\keklein", ModifiedOnDateTime=System.DateTime.Now, ModifiedBy="AM\\keklein"},
                new CifsShare{Name="Test19", UncPath="\\\\Test\\Test", CmdbCi="\\\\USWA-PFSX-CF03B\\alm_repositories", ShareOwnerFunctionalArea="EISTS", ShareOwnerCostCenter="1024", OwnerGroup="test-sharemgmt_GK", ReadOnlyGroup="test-sharemgmt_RO", ReadWriteGroup="test-sharemgmt_RW", NoChangeGroup="test-sharemgmt_NC", Status=Status.InService, OuID=1, CreatedOnDateTime=System.DateTime.Now, CreatedBy="AM\\keklein", ModifiedOnDateTime=System.DateTime.Now, ModifiedBy="AM\\keklein"},
            };
            shares.ForEach(s=> context.CifsShares.Add(s));
            context.SaveChanges();

            var requests = new List<CifsPermissionRequest>
            {
                new CifsPermissionRequest{CifsShareID = 1, PermissionType=0, RequestType=0, RequestedByUserAlias="keklein",RequestedByUserName="Klein, Kevin Alexander", RequestedForUserAlias="keklein", RequestedForUserName="Klein, Kevin Alexander", RequestOpenedOnDateTime=System.DateTime.Now, RequestJustification="Test 1", RequestStatus=0},
                new CifsPermissionRequest{CifsShareID = 1, PermissionType=0, RequestType=0, RequestedByUserAlias="keklein",RequestedByUserName="Klein, Kevin Alexander", RequestedForUserAlias="keklein", RequestedForUserName="Klein, Kevin Alexander", RequestOpenedOnDateTime=System.DateTime.Now, RequestJustification="Test 2", RequestStatus=0},
                new CifsPermissionRequest{CifsShareID = 2, PermissionType=0, RequestType=0, RequestedByUserAlias="keklein",RequestedByUserName="Klein, Kevin Alexander", RequestedForUserAlias="keklein", RequestedForUserName="Klein, Kevin Alexander", RequestOpenedOnDateTime=System.DateTime.Now, RequestJustification="Test 2", RequestStatus=0},
                new CifsPermissionRequest{CifsShareID = 5, PermissionType=0, RequestType=0, RequestedByUserAlias="keklein",RequestedByUserName="Klein, Kevin Alexander", RequestedForUserAlias="keklein", RequestedForUserName="Klein, Kevin Alexander", RequestOpenedOnDateTime=System.DateTime.Now, RequestJustification="Test 2", RequestStatus=0}
            };
            requests.ForEach(s => context.CifsPermissionRequests.Add(s));
            context.SaveChanges();

        }
    }
}