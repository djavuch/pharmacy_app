import { useEffect, useState } from "react";
import { Edit, Plus, Trash2 } from "lucide-react";
import { adminCategoryApi } from "@/shared/api";
import type { CategoryDto } from "@/shared/types";
import { TabsContent } from "@/components/ui/tabs";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import {
  Dialog,
  DialogContent,
  DialogFooter,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
} from "@/components/ui/dialog";
import { Label } from "@/components/ui/label";
import { Input } from "@/components/ui/input";
import { Textarea } from "@/components/ui/textarea";

export function CategoriesTab() {
  const [categories, setCategories] = useState<CategoryDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [reloadToken, setReloadToken] = useState(0);
  const [page, setPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const pageSize = 15;

  const refreshCategories = () => {
    setReloadToken((value) => value + 1);
  };

  useEffect(() => {
    const fetch = async () => {
      setLoading(true);
      setError(null);

      try {
        const result = await adminCategoryApi.getAll({ pageIndex: page, pageSize });
        setCategories(result.items ?? []);
        setTotalPages(result.totalPages ?? 1);
      } catch (err) {
        setCategories([]);
        setTotalPages(1);
        setError(err instanceof Error ? err.message : "Failed to load categories");
      } finally {
        setLoading(false);
      }
    };
    void fetch();
  }, [page, pageSize, reloadToken]);

  return (
    <TabsContent value="categories">
      <Card>
        <CardHeader>
          <div className="flex items-center justify-between">
            <div>
              <CardTitle>Category Management</CardTitle>
              <CardDescription>Add and edit categories</CardDescription>
            </div>
            <AddCategoryDialog onAdded={refreshCategories} />
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
                    <TableHead>Description</TableHead>
                    <TableHead className="text-right">Actions</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {categories.map((category) => (
                    <TableRow key={category.categoryId}>
                      <TableCell>{category.categoryId}</TableCell>
                      <TableCell className="font-medium">{category.categoryName}</TableCell>
                      <TableCell>{category.categoryDescription}</TableCell>
                      <TableCell className="text-right">
                        <div className="flex justify-end gap-2">
                          <Button variant="ghost" size="icon">
                            <Edit className="h-4 w-4" />
                          </Button>
                          <Button variant="ghost" size="icon">
                            <Trash2 className="h-4 w-4" />
                          </Button>
                        </div>
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>

              {categories.length === 0 && (
                <p className="py-8 text-center text-muted-foreground">
                  No categories found.
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
    </TabsContent>
  );
}

function AddCategoryDialog({ onAdded }: { onAdded: () => void }) {
  const [open, setOpen] = useState(false);
  const [name, setName] = useState("");
  const [description, setDescription] = useState("");
  const [loading, setLoading] = useState(false);

  const handleCreate = async () => {
    setLoading(true);
    try {
      await adminCategoryApi.create({
        categoryName: name,
        categoryDescription: description || undefined,
      });
      setOpen(false);
      onAdded();
    } catch (err) {
      alert(err instanceof Error ? err.message : "Error creating");
    } finally {
      setLoading(false);
    }
  };

  return (
    <Dialog open={open} onOpenChange={setOpen}>
      <DialogTrigger
        render={(
          <Button>
            <Plus className="mr-2 h-4 w-4" />
            Add Category
          </Button>
        )}
      />
      <DialogContent>
        <DialogHeader>
          <DialogTitle>New Category</DialogTitle>
        </DialogHeader>
        <div className="space-y-4">
          <div className="space-y-2">
            <Label htmlFor="catName">Name *</Label>
            <Input id="catName" value={name} onChange={(event) => setName(event.target.value)} required />
          </div>
          <div className="space-y-2">
            <Label htmlFor="catDesc">Description</Label>
            <Textarea id="catDesc" value={description} onChange={(event) => setDescription(event.target.value)} />
          </div>
        </div>
        <DialogFooter>
          <Button variant="outline" onClick={() => setOpen(false)}>Cancel</Button>
          <Button onClick={handleCreate} disabled={loading}>{loading ? "Creating..." : "Create"}</Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
