using System;
using Ardalis.Specification;
using Microsoft.eShopWeb.ApplicationCore.Entities;

namespace Microsoft.eShopWeb.ApplicationCore.Specifications;

public record CatalogListing(int Id, string Name, string PictureUri, decimal Price);

public class CatalogListingSpecification : Specification<CatalogItem, CatalogListing>
{
    public CatalogListingSpecification(int skip, int take, int? brandId, int? typeId)
    {
        if (take == 0)
        {
            take = int.MaxValue;
        }

        Query
            .Where(i => (!brandId.HasValue || i.CatalogBrandId == brandId) &&
            (!typeId.HasValue || i.CatalogTypeId == typeId))
            .Skip(skip).Take(take);

        Query.Select(i => new CatalogListing(
            i.Id,
            i.Name,
            i.PictureUri,
            i.CatalogBrand != null && i.CatalogBrand.Brand == "Brightpath Learning"
                ? Math.Round(i.Price * 0.9m, 2)
                : i.Price));
    }
}
