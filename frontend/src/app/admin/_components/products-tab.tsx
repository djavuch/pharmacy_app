import { useEffect, useState } from "react";
import { usePathname, useRouter, useSearchParams } from "next/navigation";
import { Edit, Eye, Plus, Search, Trash2, Upload } from "lucide-react";
import { adminCategoryApi, adminProductApi } from "@/shared/api";
import type { CategoryDto, ProductDto } from "@/shared/types";
import { TabsContent } from "@/components/ui/tabs";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { Badge } from "@/components/ui/badge";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
} from "@/components/ui/dialog";
import { Textarea } from "@/components/ui/textarea";
import { buildUrlWithQuery, getCurrentUrl, parsePositiveIntParam } from "./query-state";

export function ProductsTab({ isActive }: { isActive: boolean }) {
  const router = useRouter();
  const pathname = usePathname();
  const searchParams = useSearchParams();
  const [products, setProducts] = useState<ProductDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [search, setSearch] = useState(() => searchParams.get("productsSearch") ?? "");
  const [reloadToken, setReloadToken] = useState(0);
  const [page, setPage] = useState(() =>
    parsePositiveIntParam(searchParams.get("productsPage"), 1),
  );
  const [totalPages, setTotalPages] = useState(1);
  const pageSize = 15;
  const [categories, setCategories] = useState<CategoryDto[]>([]);
  const [categoriesLoading, setCategoriesLoading] = useState(true);
  const [categoriesError, setCategoriesError] = useState<string | null>(null);

  const [viewProduct, setViewProduct] = useState<ProductDto | null>(null);
  const [editingProduct, setEditingProduct] = useState<ProductDto | null>(null);
  const [deletingProduct, setDeletingProduct] = useState<ProductDto | null>(null);

  const [editName, setEditName] = useState("");
  const [editDescription, setEditDescription] = useState("");
  const [editCategoryId, setEditCategoryId] = useState("");
  const [editPrice, setEditPrice] = useState("");
  const [editStockQuantity, setEditStockQuantity] = useState("");
  const [editImageFile, setEditImageFile] = useState<File | null>(null);
  const [savingEdit, setSavingEdit] = useState(false);
  const [deleting, setDeleting] = useState(false);

  useEffect(() => {
    if (!isActive) {
      return;
    }

    const nextUrl = buildUrlWithQuery(pathname, searchParams, {
      tab: "products",
      productsSearch: search || undefined,
      productsPage: page > 1 ? page : undefined,
    });
    const currentUrl = getCurrentUrl(pathname, searchParams);

    if (nextUrl !== currentUrl) {
      router.replace(nextUrl, { scroll: false });
    }
  }, [isActive, page, pathname, router, search, searchParams]);

  useEffect(() => {
    const fetch = async () => {
      setLoading(true);
      setError(null);
      try {
        const trimmedSearch = search.trim();
        const result = await adminProductApi.getAll({
          pageIndex: page,
          pageSize,
          filterOn: trimmedSearch ? "name" : undefined,
          filterQuery: trimmedSearch || undefined,
        });
        setProducts(result.data?.items ?? []);
        setTotalPages(result.data?.totalPages ?? 1);
      } catch (err) {
        setProducts([]);
        setTotalPages(1);
        setError(err instanceof Error ? err.message : "Failed to load products");
      } finally {
        setLoading(false);
      }
    };

    void fetch();
  }, [page, pageSize, reloadToken, search]);

  useEffect(() => {
    let ignore = false;

    const fetchCategories = async () => {
      setCategoriesLoading(true);
      setCategoriesError(null);

      try {
        const aggregated: CategoryDto[] = [];
        const categoriesPageSize = 100;
        let nextPage = 1;
        let totalPages = 1;

        while (nextPage <= totalPages) {
          const result = await adminCategoryApi.getAll({
            pageIndex: nextPage,
            pageSize: categoriesPageSize,
          });
          aggregated.push(...(result.items ?? []));
          totalPages = result.totalPages ?? nextPage;
          nextPage += 1;
        }

        const uniqueById = new Map<number, CategoryDto>();
        for (const entry of aggregated) {
          uniqueById.set(entry.categoryId, entry);
        }

        const sorted = [...uniqueById.values()].sort((left, right) =>
          left.categoryName.localeCompare(right.categoryName),
        );

        if (!ignore) {
          setCategories(sorted);
        }
      } catch (err) {
        if (!ignore) {
          setCategories([]);
          setCategoriesError(err instanceof Error ? err.message : "Failed to load categories");
        }
      } finally {
        if (!ignore) {
          setCategoriesLoading(false);
        }
      }
    };

    void fetchCategories();

    return () => {
      ignore = true;
    };
  }, []);

  useEffect(() => {
    if (!editingProduct) {
      setEditImageFile(null);
      return;
    }

    setEditName(editingProduct.name);
    setEditDescription(editingProduct.description ?? "");
    setEditCategoryId(String(editingProduct.categoryId));
    setEditPrice(String(editingProduct.price));
    setEditStockQuantity(String(editingProduct.stockQuantity));
    setEditImageFile(null);
  }, [editingProduct]);

  const refreshProducts = () => {
    setReloadToken((value) => value + 1);
  };

  const resolveCategoryLabel = (value: unknown, fallback: string): string => {
    if (value == null || value === "") {
      return fallback;
    }

    const categoryId = Number(value);
    if (Number.isNaN(categoryId)) {
      return String(value);
    }

    const category = categories.find((entry) => entry.categoryId === categoryId);
    return category?.categoryName ?? `Category #${categoryId}`;
  };

  const handleSaveEdit = async () => {
    if (!editingProduct) return;

    const name = editName.trim();
    const categoryId = Number.parseInt(editCategoryId, 10);
    const price = Number.parseFloat(editPrice);
    const stockQuantity = Number.parseInt(editStockQuantity, 10);

    if (!name) {
      setError("Product name is required");
      return;
    }

    if (Number.isNaN(categoryId) || categoryId <= 0) {
      setError("Category is required");
      return;
    }

    if (Number.isNaN(price) || price < 0) {
      setError("Price must be a non-negative number");
      return;
    }

    if (Number.isNaN(stockQuantity) || stockQuantity < 0) {
      setError("Stock quantity must be a non-negative integer");
      return;
    }

    setSavingEdit(true);
    setError(null);

    try {
      let imageUrl = editingProduct.imageUrl;
      if (editImageFile) {
        const uploadResult = await adminProductApi.uploadImage(editImageFile);
        imageUrl = uploadResult.url;
      }

      await adminProductApi.update({
        productId: editingProduct.id,
        name,
        description: editDescription.trim() || undefined,
        categoryId,
        price,
        stockQuantity,
        imageUrl,
      });

      const updatedProduct = await adminProductApi.getById(editingProduct.id);
      setProducts((current) =>
        current.map((entry) => (entry.id === editingProduct.id ? updatedProduct : entry)),
      );
      setViewProduct((current) =>
        current && current.id === editingProduct.id ? updatedProduct : current,
      );
      setEditingProduct(null);
      refreshProducts();
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to update product");
    } finally {
      setSavingEdit(false);
    }
  };

  const handleDelete = async () => {
    if (!deletingProduct) return;

    setDeleting(true);
    setError(null);

    try {
      await adminProductApi.delete(deletingProduct.id);
      setProducts((current) =>
        current.filter((entry) => entry.id !== deletingProduct.id),
      );
      setViewProduct((current) =>
        current && current.id === deletingProduct.id ? null : current,
      );
      setEditingProduct((current) =>
        current && current.id === deletingProduct.id ? null : current,
      );
      setDeletingProduct(null);

      if (products.length === 1 && page > 1) {
        setPage((value) => Math.max(1, value - 1));
      } else {
        refreshProducts();
      }
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to delete product");
    } finally {
      setDeleting(false);
    }
  };

  return (
    <TabsContent value="products">
      <Card>
        <CardHeader>
          <div className="flex items-center justify-between">
            <div>
              <CardTitle>Product Management</CardTitle>
              <CardDescription>Add and edit products in the catalog</CardDescription>
            </div>
            <AddProductDialog
              onAdded={refreshProducts}
              categories={categories}
              categoriesLoading={categoriesLoading}
              categoriesError={categoriesError}
            />
          </div>
          <div className="mt-4 flex items-center gap-2">
            <Search className="h-4 w-4 text-muted-foreground" />
            <Input
              placeholder="Search products..."
              value={search}
              onChange={(event) => {
                setSearch(event.target.value);
                setPage(1);
              }}
              className="max-w-sm"
            />
          </div>
        </CardHeader>
        <CardContent>
          {error && (
            <div className="mb-3 rounded-md border border-rose-200 bg-rose-50 px-3 py-2 text-sm text-rose-700">
              {error}
            </div>
          )}

          {loading ? (
            <p className="py-8 text-center text-muted-foreground">Loading...</p>
          ) : (
            <>
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead>ID</TableHead>
                    <TableHead>Name</TableHead>
                    <TableHead>Category</TableHead>
                    <TableHead>Price</TableHead>
                    <TableHead>Stock</TableHead>
                    <TableHead className="text-right">Actions</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {products.map((product) => (
                    <TableRow key={product.id}>
                      <TableCell>{product.id}</TableCell>
                      <TableCell className="font-medium">{product.name}</TableCell>
                      <TableCell>{product.categoryName}</TableCell>
                      <TableCell>${product.price}</TableCell>
                      <TableCell>
                        <Badge
                          variant={product.stockQuantity > 0 ? "default" : "secondary"}
                          className={product.stockQuantity > 0 ? "bg-green-100 text-green-800" : ""}
                        >
                          {product.stockQuantity > 0 ? `${product.stockQuantity} units` : "None"}
                        </Badge>
                      </TableCell>
                      <TableCell className="text-right">
                        <div className="flex justify-end gap-2">
                          <Button
                            variant="ghost"
                            size="icon"
                            title="View details"
                            onClick={() => setViewProduct(product)}
                          >
                            <Eye className="h-4 w-4" />
                          </Button>
                          <Button
                            variant="ghost"
                            size="icon"
                            title="Edit product"
                            onClick={() => setEditingProduct(product)}
                          >
                            <Edit className="h-4 w-4" />
                          </Button>
                          <Button
                            variant="ghost"
                            size="icon"
                            title="Delete product"
                            onClick={() => setDeletingProduct(product)}
                          >
                            <Trash2 className="h-4 w-4 text-rose-600" />
                          </Button>
                        </div>
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>

              {products.length === 0 && (
                <p className="py-8 text-center text-muted-foreground">
                  No products found for current filters.
                </p>
              )}

              {totalPages > 1 && (
                <div className="mt-4 flex items-center justify-center gap-2">
                  <Button
                    variant="outline"
                    size="sm"
                    disabled={page <= 1}
                    onClick={() => setPage((value) => value - 1)}
                  >
                    Previous
                  </Button>
                  <span className="text-sm text-muted-foreground">
                    {page} / {totalPages}
                  </span>
                  <Button
                    variant="outline"
                    size="sm"
                    disabled={page >= totalPages}
                    onClick={() => setPage((value) => value + 1)}
                  >
                    Next
                  </Button>
                </div>
              )}
            </>
          )}
        </CardContent>
      </Card>

      <Dialog
        open={Boolean(viewProduct)}
        onOpenChange={(isOpen) => {
          if (!isOpen) setViewProduct(null);
        }}
      >
        <DialogContent className="max-w-2xl">
          <DialogHeader>
            <DialogTitle>Product details</DialogTitle>
            <DialogDescription>
              {viewProduct ? `Product #${viewProduct.id}` : "Product"}
            </DialogDescription>
          </DialogHeader>

          {viewProduct && (
            <div className="space-y-4">
              <div className="grid gap-3 rounded-lg border bg-muted/30 p-4 md:grid-cols-2">
                <div>
                  <p className="text-xs text-muted-foreground">Name</p>
                  <p className="font-medium">{viewProduct.name}</p>
                </div>
                <div>
                  <p className="text-xs text-muted-foreground">Category</p>
                  <p className="font-medium">
                    {viewProduct.categoryName} (ID: {viewProduct.categoryId})
                  </p>
                </div>
                <div>
                  <p className="text-xs text-muted-foreground">Price</p>
                  <p className="font-medium">${viewProduct.price.toFixed(2)}</p>
                </div>
                <div>
                  <p className="text-xs text-muted-foreground">Stock</p>
                  <p className="font-medium">{viewProduct.stockQuantity} units</p>
                </div>
                <div>
                  <p className="text-xs text-muted-foreground">Rating</p>
                  <p className="font-medium">
                    {viewProduct.averageRating != null
                      ? `${viewProduct.averageRating.toFixed(1)} (${viewProduct.reviewCount ?? 0} reviews)`
                      : "No ratings yet"}
                  </p>
                </div>
                <div>
                  <p className="text-xs text-muted-foreground">Wishlist count</p>
                  <p className="font-medium">{viewProduct.wishlistCount}</p>
                </div>
              </div>

              <div className="rounded-lg border p-4">
                <p className="text-xs text-muted-foreground">Description</p>
                <p className="mt-1 text-sm text-muted-foreground">
                  {viewProduct.description?.trim() || "No description"}
                </p>
              </div>

              <div className="rounded-lg border p-4">
                <p className="text-xs text-muted-foreground">Image URL</p>
                <p className="mt-1 break-all text-sm text-muted-foreground">
                  {viewProduct.imageUrl || "Not specified"}
                </p>
              </div>
            </div>
          )}
        </DialogContent>
      </Dialog>

      <Dialog
        open={Boolean(editingProduct)}
        onOpenChange={(isOpen) => {
          if (!isOpen) {
            setEditingProduct(null);
            setEditImageFile(null);
          }
        }}
      >
        <DialogContent className="max-h-[85vh] max-w-lg overflow-y-auto">
          <DialogHeader>
            <DialogTitle>Edit product</DialogTitle>
            <DialogDescription>
              {editingProduct ? `Update product #${editingProduct.id}` : "Update product"}
            </DialogDescription>
          </DialogHeader>

          <div className="space-y-4">
            <div className="space-y-2">
              <Label htmlFor="editProductName">Name *</Label>
              <Input
                id="editProductName"
                value={editName}
                onChange={(event) => setEditName(event.target.value)}
                required
              />
            </div>

            <div className="space-y-2">
              <Label htmlFor="editProductDescription">Description</Label>
              <Textarea
                id="editProductDescription"
                value={editDescription}
                onChange={(event) => setEditDescription(event.target.value)}
              />
            </div>

            <div className="space-y-2">
              <Label htmlFor="editProductCategoryId">Category *</Label>
              <Select
                value={editCategoryId}
                onValueChange={(value) => {
                  setEditCategoryId(value ?? "");
                }}
              >
                <SelectTrigger id="editProductCategoryId" className="w-full">
                  <SelectValue placeholder={categoriesLoading ? "Loading categories..." : "Select category"}>
                    {(value) => resolveCategoryLabel(value, "Select category")}
                  </SelectValue>
                </SelectTrigger>
                <SelectContent className="w-auto min-w-(--anchor-width) max-w-[min(90vw,48rem)] overflow-x-auto">
                  {categories.map((category) => (
                    <SelectItem
                      key={`edit-category-${category.categoryId}`}
                      value={String(category.categoryId)}
                      title={category.categoryName}
                    >
                      {category.categoryName}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
              {categoriesError && (
                <p className="text-xs text-rose-700">
                  Categories are unavailable: {categoriesError}
                </p>
              )}
              {!categoriesLoading && categories.length === 0 && !categoriesError && (
                <p className="text-xs text-muted-foreground">
                  No categories found. Create a category first.
                </p>
              )}
            </div>

            <div className="grid grid-cols-2 gap-4">
              <div className="space-y-2">
                <Label htmlFor="editProductPrice">Price ($) *</Label>
                <Input
                  id="editProductPrice"
                  type="number"
                  min={0}
                  step="0.01"
                  value={editPrice}
                  onChange={(event) => setEditPrice(event.target.value)}
                  required
                />
              </div>

              <div className="space-y-2">
                <Label htmlFor="editProductStock">Quantity *</Label>
                <Input
                  id="editProductStock"
                  type="number"
                  min={0}
                  step={1}
                  value={editStockQuantity}
                  onChange={(event) => setEditStockQuantity(event.target.value)}
                  required
                />
              </div>
            </div>

            <div className="space-y-2">
              <Label htmlFor="editProductImage">New image (optional)</Label>
              <Input
                id="editProductImage"
                type="file"
                accept="image/png,image/jpeg,image/webp"
                onChange={(event) => setEditImageFile(event.target.files?.[0] ?? null)}
              />
              <p className="text-xs text-muted-foreground">
                Current image: {editingProduct?.imageUrl || "Not specified"}
              </p>
            </div>
          </div>

          <DialogFooter>
            <Button
              variant="outline"
              onClick={() => {
                setEditingProduct(null);
                setEditImageFile(null);
              }}
              disabled={savingEdit}
            >
              Cancel
            </Button>
            <Button onClick={() => void handleSaveEdit()} disabled={savingEdit}>
              {savingEdit ? "Saving..." : "Save changes"}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>

      <Dialog
        open={Boolean(deletingProduct)}
        onOpenChange={(isOpen) => {
          if (!isOpen) setDeletingProduct(null);
        }}
      >
        <DialogContent className="max-w-md">
          <DialogHeader>
            <DialogTitle>Delete product</DialogTitle>
            <DialogDescription>
              This action cannot be undone.
            </DialogDescription>
          </DialogHeader>

          <div className="rounded-md border border-rose-200 bg-rose-50 p-3 text-sm text-rose-700">
            {deletingProduct
              ? `Delete "${deletingProduct.name}" (ID: ${deletingProduct.id})?`
              : "Delete selected product?"}
          </div>

          <DialogFooter>
            <Button
              variant="outline"
              onClick={() => setDeletingProduct(null)}
              disabled={deleting}
            >
              Cancel
            </Button>
            <Button
              variant="destructive"
              onClick={() => void handleDelete()}
              disabled={deleting}
            >
              {deleting ? "Deleting..." : "Delete"}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </TabsContent>
  );
}

function AddProductDialog({
  onAdded,
  categories,
  categoriesLoading,
  categoriesError,
}: {
  onAdded: () => void;
  categories: CategoryDto[];
  categoriesLoading: boolean;
  categoriesError: string | null;
}) {
  const [open, setOpen] = useState(false);
  const [name, setName] = useState("");
  const [description, setDescription] = useState("");
  const [categoryId, setCategoryId] = useState("");
  const [price, setPrice] = useState("");
  const [stockQuantity, setStockQuantity] = useState("");
  const [imageFile, setImageFile] = useState<File | null>(null);
  const [uploadingImage, setUploadingImage] = useState(false);
  const [loading, setLoading] = useState(false);

  const resolveCategoryLabel = (value: unknown, fallback: string): string => {
    if (value == null || value === "") {
      return fallback;
    }

    const parsed = Number(value);
    if (Number.isNaN(parsed)) {
      return String(value);
    }

    const category = categories.find((entry) => entry.categoryId === parsed);
    return category?.categoryName ?? `Category #${parsed}`;
  };

  const handleCreate = async () => {
    const parsedCategoryId = Number.parseInt(categoryId, 10);
    if (Number.isNaN(parsedCategoryId) || parsedCategoryId <= 0) {
      alert("Select a valid category");
      return;
    }

    setLoading(true);
    try {
      let uploadedImageUrl: string | undefined;

      if (imageFile) {
        setUploadingImage(true);
        const uploadResult = await adminProductApi.uploadImage(imageFile);
        uploadedImageUrl = uploadResult.url;
        setUploadingImage(false);
      }

      await adminProductApi.create({
        name,
        description: description || undefined,
        categoryId: parsedCategoryId,
        price: parseFloat(price),
        stockQuantity: parseInt(stockQuantity, 10),
        imageUrl: uploadedImageUrl,
      });

      setOpen(false);
      onAdded();
    } catch (err) {
      alert(err instanceof Error ? err.message : "Error creating");
    } finally {
      setUploadingImage(false);
      setLoading(false);
    }
  };

  return (
    <Dialog open={open} onOpenChange={setOpen}>
      <DialogTrigger
        render={(
          <Button>
            <Plus className="mr-2 h-4 w-4" />
            Add Product
          </Button>
        )}
      />
      <DialogContent className="max-w-lg">
        <DialogHeader>
          <DialogTitle>New Product</DialogTitle>
          <DialogDescription>Fill in product information</DialogDescription>
        </DialogHeader>
        <div className="space-y-4">
          <div className="space-y-2">
            <Label htmlFor="name">Name *</Label>
            <Input id="name" value={name} onChange={(event) => setName(event.target.value)} required />
          </div>
          <div className="space-y-2">
            <Label htmlFor="desc">Description</Label>
            <Textarea id="desc" value={description} onChange={(event) => setDescription(event.target.value)} />
          </div>
          <div className="space-y-2">
            <Label htmlFor="cat">Category *</Label>
            <Select
              value={categoryId}
              onValueChange={(value) => {
                setCategoryId(value ?? "");
              }}
            >
              <SelectTrigger id="cat" className="w-full">
                <SelectValue placeholder={categoriesLoading ? "Loading categories..." : "Select category"}>
                  {(value) => resolveCategoryLabel(value, "Select category")}
                </SelectValue>
              </SelectTrigger>
              <SelectContent className="w-auto min-w-(--anchor-width) max-w-[min(90vw,48rem)] overflow-x-auto">
                {categories.map((category) => (
                  <SelectItem
                    key={`add-category-${category.categoryId}`}
                    value={String(category.categoryId)}
                    title={category.categoryName}
                  >
                    {category.categoryName}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
            {categoriesError && (
              <p className="text-xs text-rose-700">
                Categories are unavailable: {categoriesError}
              </p>
            )}
            {!categoriesLoading && categories.length === 0 && !categoriesError && (
              <p className="text-xs text-muted-foreground">
                No categories found. Create a category first.
              </p>
            )}
          </div>
          <div className="grid grid-cols-2 gap-4">
            <div className="space-y-2">
              <Label htmlFor="price">Price ($) *</Label>
              <Input id="price" type="number" value={price} onChange={(event) => setPrice(event.target.value)} required />
            </div>
            <div className="space-y-2">
              <Label htmlFor="stock">Quantity *</Label>
              <Input id="stock" type="number" value={stockQuantity} onChange={(event) => setStockQuantity(event.target.value)} required />
            </div>
          </div>
          <div className="space-y-2">
            <Label htmlFor="imgFile">Image</Label>
            <Input
              id="imgFile"
              type="file"
              accept="image/png,image/jpeg,image/webp"
              onChange={(event) => setImageFile(event.target.files?.[0] ?? null)}
            />
          </div>
        </div>
        <DialogFooter>
          <Button variant="outline" onClick={() => setOpen(false)}>Cancel</Button>
          <Button
            onClick={handleCreate}
            disabled={loading || uploadingImage || categoriesLoading || categories.length === 0 || !categoryId}
          >
            {uploadingImage ? (
              <>
                <Upload className="mr-2 h-4 w-4 animate-spin" />
                Uploading...
              </>
            ) : loading ? (
              "Creating..."
            ) : (
              "Create"
            )}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
