import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";

export function NewItemPage() {
  return (
    <Card>
      <CardHeader>
        <CardTitle>New Item</CardTitle>
        <CardDescription>
          This screen becomes the item intake workflow in Phase 2.
        </CardDescription>
      </CardHeader>
      <CardContent className="text-sm text-muted-foreground">
        Expect a structured form for electronics details, condition, pricing, and photos.
      </CardContent>
    </Card>
  );
}
